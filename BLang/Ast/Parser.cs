namespace BLang.Ast;

using System;
using System.Collections.Generic;
using BLang.Ast.Nodes;
using BLang.Exceptions;
using BLang.Tokenizer;
using BLang.Utility;

public partial class Parser(CompilationData data)
{
    IEnumerator<Token> tokens = null!;
    SourceRange previousTokenRange = SourceRange.Zero;
    readonly SymbolTable symbols = data.Symbols;

    public CompilationUnit Parse(IEnumerator<Token> tokens)
    {
        this.tokens = tokens;

        if (options.Tokens)
        {
            Console.WriteLine(tokens.Current);
        }

        tokens.MoveNext();
        return ParseTopLevel();
    }

    CompilationUnit ParseTopLevel()
    {
        List<FunctionDecleration> functions = [];
        List<GlobalVariable> globals = [];

        SourceRange start = previousTokenRange;

        EatComments();

        while (Peek(TokenType.Identifier) && !Peek(TokenType.Eof) && !Peek(TokenType.Garbage))
        {
            EatComments();

            Token identifier = Eat(TokenType.Identifier);

            if (Peek(TokenType.OpenParenthesis))
            {
                functions.Add(ParseFunctionDecleration(identifier));
            }
            else if (Peek(TokenType.Semicolon))
            {
                Symbol symbol = symbols.Add(identifier.Content, SymbolKind.Define);
                globals.Add(new(symbol, null));
                Eat(TokenType.Semicolon);
            }
            else if (Peek(TokenType.IntegerLiteral))
            {
                Symbol symbol = symbols.Add(identifier.Content, SymbolKind.Define);
                Token number = Eat(TokenType.IntegerLiteral);
                globals.Add(new(symbol, number.ToInteger()));
                Eat(TokenType.Semicolon);
            }
            else
            {
                Next();
                break;
            }

            EatComments();
        }

        return new(functions, globals)
        {
            Range = start.Merge(previousTokenRange)
        };
    }

    FunctionDecleration ParseFunctionDecleration(Token identifier)
    {
        symbols.EnterScope(identifier.Content);

        Symbol symbol = symbols.Add(identifier.Content, SymbolKind.Define);

        SourceRange begin = identifier.Range;

        List<Variable> parameters = [];

        Eat(TokenType.OpenParenthesis);
        while (!Peek(TokenType.CloseParenthesis) && !Peek(TokenType.Eof))
        {
            Token variable = Eat(TokenType.Identifier);
            symbol = symbols.Add(variable.Content, SymbolKind.Define);
            parameters.Add(symbol);
        }
        Eat(TokenType.CloseParenthesis);

        Statement[] body = ParseBlock();

        symbols.ExitScope();
        return new FunctionDecleration(symbol, parameters.ToArray(), body)
        {
            Range = begin.Merge(previousTokenRange),
        };
    }

    Statement[] ParseBlock()
    {
        List<Statement> statements = [];

        Eat(TokenType.OpenScope);
        while (!Peek(TokenType.CloseScope))
        {
            Statement? statement = ParseStatement();

            if (statement != null)
            {
                statements.Add(statement);
            }
            else
            {
                break;
            }
        }
        Eat(TokenType.CloseScope);

        return statements.ToArray();
    }

    Statement? ParseStatement()
    {
        return Peek() switch
        {
            TokenType.ExternKeyword => ParseExternalDefinition(),
            TokenType.WhileKeyword => ParseWhileDefinition(),
            TokenType.IfKeyword => ParseIfDefinition(),
            TokenType.AutoKeyword => ParseAutoDefinition(),
            TokenType.Identifier => ParseIdentifierStatement(),
            _ => throw new InvalidTokenException($"{data.GetFileLocation(previousTokenRange.End)} Unexpected token in {nameof(ParseStatement)} of type {Peek()}")
        };
    }

    WhileStatement ParseWhileDefinition()
    {
        Token start = Eat(TokenType.WhileKeyword);

        Expression condition = ParseBinaryExpression();
        if (condition is not BinaryExpression)
        {
            throw new ParserException("ParseWhileDefinition condition: " + condition);
        }
        Statement[] body = ParseBlock();

        return new((BinaryExpression)condition, body)
        {
            Range = start.Range.Merge(previousTokenRange),
        };
    }

    IfStatement ParseIfDefinition()
    {
        Token start = Eat(TokenType.IfKeyword);

        Expression condition = ParseBinaryExpression();
        if (condition is not BinaryExpression)
        {
            throw new ParserException("ParseWhileDefinition condition: " + condition);
        }

        Statement[] body = ParseBlock();

        Statement[]? elseBody = null;
        if (Peek(TokenType.ElseKeyword))
        {
            Eat(TokenType.ElseKeyword);
            elseBody = ParseBlock();
        }

        return new((BinaryExpression)condition, body, elseBody)
        {
            Range = start.Range.Merge(previousTokenRange),
        };
    }

    ExternalStatement ParseExternalDefinition()
    {
        Token start = Eat(TokenType.ExternKeyword);
        Token identifier = Eat(TokenType.Identifier);

        List<Symbol> externs = [symbols.Add(identifier, SymbolKind.Define)];
        while (Peek(TokenType.Comma))
        {
            Eat(TokenType.Comma);
            identifier = Eat(TokenType.Identifier);
            externs.Add(symbols.Add(identifier, SymbolKind.Define));
        }

        Token end = Eat(TokenType.Semicolon);

        return new ExternalStatement(externs.ToArray())
        {
            Range = start.Range.Merge(end.Range),
        };
    }

    AutoStatement ParseAutoDefinition()
    {
        Token start = Eat(TokenType.AutoKeyword);
        Token identifier = Eat(TokenType.Identifier);

        List<Symbol> variables = [symbols.Add(identifier, SymbolKind.Define)];
        while (Peek(TokenType.Comma))
        {
            Eat(TokenType.Comma);
            identifier = Eat(TokenType.Identifier);
            variables.Add(symbols.Add(identifier, SymbolKind.Define));
        }
        Eat(TokenType.Semicolon);

        return new AutoStatement(variables.ToArray())
        {
            Range = start.Range.Merge(previousTokenRange),
        };
    }

    Statement? ParseIdentifierStatement()
    {
        Token identifier = Eat(TokenType.Identifier);

        List<Expression> parameters = [];

        return Peek() switch
        {
            TokenType.OpenParenthesis => ParseFunctionCall(identifier, parameters),
            TokenType.Assignment => ParseVariableAssignment(identifier),
            _ => throw new ParserException($"{data.GetFileLocation(previousTokenRange.End)} Unexpected token in {nameof(ParseIdentifierStatement)} of type {Peek()}"),
        };
    }

    FunctionCall ParseFunctionCall(Token identifier, List<Expression> parameters)
    {
        Eat(TokenType.OpenParenthesis);
        while (!Peek(TokenType.CloseParenthesis))
        {
            parameters.Add(ParseParameter());
            if (Peek(TokenType.Comma))
            {
                Eat(TokenType.Comma);
            }
        }
        Eat(TokenType.CloseParenthesis);
        Eat(TokenType.Semicolon);

        Symbol symbol = symbols.GetOrAdd(identifier, SymbolKind.Load);

        return new FunctionCall(symbol, parameters.ToArray())
        {
            Range = identifier.Range,
        };
    }

    Expression ParseParameter()
    {
        return Peek() switch
        {
            TokenType.StringLiteral => ParseString(),
            TokenType.IntegerLiteral => ParseInteger(),
            TokenType.Identifier => ParseVariable(),
            _ => throw new ParserException($"{data.GetFileLocation(previousTokenRange.End)} ParseExpression: {Peek()}"),
        };
    }

    VariableDeclarator ParseVariableAssignment(Token identifier)
    {
        Eat(TokenType.Assignment);
        Expression value = ParseBinaryExpression();
        Eat(TokenType.Semicolon);

        Symbol symbol = symbols.GetOrAdd(identifier, SymbolKind.Assign);

        return new VariableDeclarator(symbol, value)
        {
            Range = identifier.Range,
        };
    }
}
