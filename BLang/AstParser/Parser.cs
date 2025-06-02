namespace BLang.AstParser;

using System;
using System.Collections.Generic;
using BLang.Exceptions;
using BLang.Tokenizer;
using BLang.Utility;

public partial class Parser(CompilationData data)
{
    IEnumerator<Token> tokens = null!;

    public CompilationUnit Parse(IEnumerator<Token> tokens, string file)
    {
        this.tokens = tokens;

        tokens.MoveNext();
        if (Flags.Instance.Tokens)
        {
            Console.WriteLine(tokens.Current);
        }

        CompilationUnit topLevel = ParseTopLevel();
        topLevel.File = file;
        return topLevel;
    }

    CompilationUnit ParseTopLevel()
    {
        List<FunctionStatement> functions = [];

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
                throw new ParserException("TODO: global variables");
            }
            else
            {
                Next();
                Error($"Expected '(' or ';' but got {Peek()}");
                break;
            }

            EatComments();
        }

        return new(functions, [])
        {
            Range = new()
        };
    }

    FunctionStatement ParseFunctionDecleration(Token identifier)
    {
        SourceRange begin = identifier.Range;

        List<Variable> parameters = [];

        Eat(TokenType.OpenParenthesis);
        while (!Peek(TokenType.CloseParenthesis) && !Peek(TokenType.Eof))
        {
            Token variable = Eat(TokenType.Identifier);
            parameters.Add(new Variable(variable.Content) { Range = variable.Range });
        }
        Eat(TokenType.CloseParenthesis);

        Statement[] body = ParseBlock();

        return new FunctionStatement(identifier.Content, parameters.ToArray(), body)
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
            TokenType.AutoKeyword => ParseAutoDefinition(),
            TokenType.Identifier => ParseIdentifierStatement(),
            _ => throw new InvalidTokenException($"{data.GetFileLocation(previousTokenRange.End)} Unexpected token in {nameof(ParseStatement)} of type {Peek()}")
        };
    }

    WhileStatement ParseWhileDefinition()
    {
        Token start = Eat(TokenType.WhileKeyword);

        BinaryExpression condition = ParseBinaryExpression();
        Statement[] body = ParseBlock();

        return new(condition, body)
        {
            Range = start.Range.Merge(previousTokenRange),
        };
    }


    ExternalStatement ParseExternalDefinition()
    {
        Token start = Eat(TokenType.ExternKeyword);

        Token identifier = Eat(TokenType.Identifier);

        List<string> externs = [identifier.Content];
        if (Peek(TokenType.Comma))
        {
            Eat(TokenType.Comma);
            identifier = Eat(TokenType.Identifier);
            externs.Add(identifier.Content);
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

        List<string> variables = [identifier.Content];
        while (Peek(TokenType.Comma))
        {
            Eat(TokenType.Comma);
            identifier = Eat(TokenType.Identifier);
            variables.Add(identifier.Content);
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

        return new FunctionCall(identifier.Content, parameters.ToArray())
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

    VariableAssignment ParseVariableAssignment(Token identifier)
    {
        Eat(TokenType.Assignment);
        BinaryExpression value = ParseBinaryExpression();
        Eat(TokenType.Semicolon);

        return new VariableAssignment(identifier.Content, value)
        {
            Range = identifier.Range,
        };
    }
}
