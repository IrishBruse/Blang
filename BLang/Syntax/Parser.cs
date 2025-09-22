namespace BLang.Ast;

using System;
using System.Collections.Generic;

using BLang.Ast.Nodes;
using BLang.Exceptions;
using BLang.Tokenizer;
using BLang.Utility;

public partial class Parser(CompilerContext data)
{
    private IEnumerator<Token> tokens = null!;
    private SourceRange previousTokenRange = SourceRange.Zero;
    private readonly SymbolTable symbols = data.Symbols;

    public CompilationUnit Parse(IEnumerator<Token> tokens)
    {
        this.tokens = tokens;

        _ = tokens.MoveNext();
        if (Options.Tokens)
        {
            Console.WriteLine(tokens.Current);
        }

        return ParseTopLevel();
    }

    private CompilationUnit ParseTopLevel()
    {
        List<FunctionDecleration> functions = [];
        List<VariableDecleration> globals = [];

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
                _ = Eat(TokenType.Semicolon);
            }
            else if (Peek(TokenType.IntegerLiteral))
            {
                Symbol symbol = symbols.Add(identifier.Content, SymbolKind.Define);
                Token number = Eat(TokenType.IntegerLiteral);
                globals.Add(new(symbol, number.ToInteger()));
                _ = Eat(TokenType.Semicolon);
            }
            else
            {
                _ = Next();
                break;
            }

            EatComments();
        }

        return new(functions.ToArray(), globals.ToArray())
        {
            Range = start.Merge(previousTokenRange)
        };
    }

    private FunctionDecleration ParseFunctionDecleration(Token identifier)
    {
        symbols.EnterScope(identifier.Content);

        Symbol symbol = symbols.Add(identifier.Content, SymbolKind.Define);

        SourceRange begin = identifier.Range;

        List<Variable> parameters = [];

        _ = Eat(TokenType.OpenParenthesis);
        while (!Peek(TokenType.CloseParenthesis) && !Peek(TokenType.Eof))
        {
            Token variable = Eat(TokenType.Identifier);
            symbol = symbols.Add(variable.Content, SymbolKind.Define);
            parameters.Add(symbol);

            if (Peek(TokenType.Comma))
            {
                _ = Eat(TokenType.Comma);
            }
        }
        _ = Eat(TokenType.CloseParenthesis);

        Statement[] body = ParseBlock();

        symbols.ExitScope();
        return new FunctionDecleration(symbol, parameters.ToArray(), body)
        {
            Range = begin.Merge(previousTokenRange),
        };
    }

    private Statement[] ParseBlock()
    {
        List<Statement> statements = [];

        _ = Eat(TokenType.OpenScope);
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
        _ = Eat(TokenType.CloseScope);

        return statements.ToArray();
    }

    private Statement ParseStatement()
    {
        return Peek() switch
        {
            TokenType.ExternKeyword => ParseExternalDefinition(),
            TokenType.WhileKeyword => ParseWhileDefinition(),
            TokenType.SwitchKeyword => ParseWhileDefinition(),
            TokenType.IfKeyword => ParseIfDefinition(),
            TokenType.AutoKeyword => ParseAutoDefinition(),
            TokenType.Identifier => ParseIdentifierStatement(),
            TokenType.Eof => throw new NotImplementedException(),
            TokenType.Garbage => throw new NotImplementedException(),
            TokenType.None => throw new NotImplementedException(),
            TokenType.Comment => throw new NotImplementedException(),
            TokenType.IntegerLiteral => throw new NotImplementedException(),
            TokenType.FloatLiteral => throw new NotImplementedException(),
            TokenType.StringLiteral => throw new NotImplementedException(),
            TokenType.CharLiteral => throw new NotImplementedException(),
            TokenType.OpenParenthesis => throw new NotImplementedException(),
            TokenType.CloseParenthesis => throw new NotImplementedException(),
            TokenType.OpenBracket => throw new NotImplementedException(),
            TokenType.CloseBracket => throw new NotImplementedException(),
            TokenType.OpenScope => throw new NotImplementedException(),
            TokenType.CloseScope => throw new NotImplementedException(),
            TokenType.Dot => throw new NotImplementedException(),
            TokenType.Comma => throw new NotImplementedException(),
            TokenType.Addition => throw new NotImplementedException(),
            TokenType.Subtraction => throw new NotImplementedException(),
            TokenType.Multiplication => throw new NotImplementedException(),
            TokenType.Division => throw new NotImplementedException(),
            TokenType.Modulo => throw new NotImplementedException(),
            TokenType.LessThan => throw new NotImplementedException(),
            TokenType.GreaterThan => throw new NotImplementedException(),
            TokenType.LessThanEqual => throw new NotImplementedException(),
            TokenType.GreaterThanEqual => throw new NotImplementedException(),
            TokenType.EqualEqual => throw new NotImplementedException(),
            TokenType.NotEqual => throw new NotImplementedException(),
            TokenType.LogicalAnd => throw new NotImplementedException(),
            TokenType.LogicalOr => throw new NotImplementedException(),
            TokenType.LogicalNot => throw new NotImplementedException(),
            TokenType.Assignment => throw new NotImplementedException(),
            TokenType.AdditionAssignment => throw new NotImplementedException(),
            TokenType.SubtractionAssignment => throw new NotImplementedException(),
            TokenType.MultiplicationAssignment => throw new NotImplementedException(),
            TokenType.DivisionAssignment => throw new NotImplementedException(),
            TokenType.ModuloAssignment => throw new NotImplementedException(),
            TokenType.Increment => throw new NotImplementedException(),
            TokenType.Decrement => throw new NotImplementedException(),
            TokenType.BitwiseComplement => throw new NotImplementedException(),
            TokenType.BitwiseAnd => throw new NotImplementedException(),
            TokenType.BitwiseOr => throw new NotImplementedException(),
            TokenType.BitwiseXOr => throw new NotImplementedException(),
            TokenType.BitwiseShiftLeft => throw new NotImplementedException(),
            TokenType.BitwiseShiftRight => throw new NotImplementedException(),
            TokenType.Semicolon => throw new NotImplementedException(),
            TokenType.ElseKeyword => throw new NotImplementedException(),
            TokenType.CaseKeyword => throw new NotImplementedException(),
            TokenType.BreakKeyword => throw new NotImplementedException(),
            TokenType.ArrayIndexing => throw new NotImplementedException(),
            _ => throw new InvalidTokenException($"{data.GetFileLocation(previousTokenRange.End)} Unexpected token in {nameof(ParseStatement)} of type {Peek()}")
        };
    }

    private WhileStatement ParseWhileDefinition()
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

    private SwitchStatement ParseSwitchDefinition()
    {
        Token start = Eat(TokenType.SwitchKeyword);

        Expression condition = ParseBinaryExpression();
        if (condition is not BinaryExpression)
        {
            throw new ParserException("ParseSwitchDefinition condition: " + condition);
        }
        Statement[] body = ParseBlock();

        return new((BinaryExpression)condition, body)
        {
            Range = start.Range.Merge(previousTokenRange),
        };
    }

    private IfStatement ParseIfDefinition()
    {
        Token start = Eat(TokenType.IfKeyword);

        Expression condition = ParseBinaryExpression();
        if (condition is not BinaryExpression)
        {
            throw new ParserException("ParseWhileDefinition condition: " + condition);
        }

        Statement[] body = !Peek(TokenType.OpenScope) ? [ParseStatement()] : ParseBlock();
        Statement[]? elseBody = null;
        if (Peek(TokenType.ElseKeyword))
        {
            _ = Eat(TokenType.ElseKeyword);
            elseBody = ParseBlock();
        }

        return new((BinaryExpression)condition, body, elseBody)
        {
            Range = start.Range.Merge(previousTokenRange),
        };
    }

    private ExternalStatement ParseExternalDefinition()
    {
        Token start = Eat(TokenType.ExternKeyword);
        Token identifier = Eat(TokenType.Identifier);

        List<Symbol> externs = [symbols.Add(identifier, SymbolKind.Define)];
        while (Peek(TokenType.Comma))
        {
            _ = Eat(TokenType.Comma);
            identifier = Eat(TokenType.Identifier);
            externs.Add(symbols.Add(identifier, SymbolKind.Define));
        }

        Token end = Eat(TokenType.Semicolon);

        return new ExternalStatement(externs.ToArray())
        {
            Range = start.Range.Merge(end.Range),
        };
    }

    private AutoStatement ParseAutoDefinition()
    {
        Token start = Eat(TokenType.AutoKeyword);
        Token identifier = Eat(TokenType.Identifier);

        List<Symbol> variables = [symbols.Add(identifier, SymbolKind.Define)];
        while (Peek(TokenType.Comma))
        {
            _ = Eat(TokenType.Comma);
            identifier = Eat(TokenType.Identifier);
            variables.Add(symbols.Add(identifier, SymbolKind.Define));
        }
        _ = Eat(TokenType.Semicolon);

        return new AutoStatement(variables.ToArray())
        {
            Range = start.Range.Merge(previousTokenRange),
        };
    }

    private Statement ParseIdentifierStatement()
    {
        Token identifier = Eat(TokenType.Identifier);

        List<Expression> parameters = [];

        TokenType token = Peek();
        _ = Eat(token);
        return token switch
        {
            TokenType.OpenParenthesis => ParseFunctionCall(identifier, parameters),
            TokenType.Assignment => ParseVariableAssignment(identifier),
            TokenType.Increment => ParseVariableAssignmentShorthand(identifier, new IntValue(1)),
            TokenType.Eof => throw new NotImplementedException(),
            TokenType.Garbage => throw new NotImplementedException(),
            TokenType.None => throw new NotImplementedException(),
            TokenType.Comment => throw new NotImplementedException(),
            TokenType.Identifier => throw new NotImplementedException(),
            TokenType.IntegerLiteral => throw new NotImplementedException(),
            TokenType.FloatLiteral => throw new NotImplementedException(),
            TokenType.StringLiteral => throw new NotImplementedException(),
            TokenType.CharLiteral => throw new NotImplementedException(),
            TokenType.CloseParenthesis => throw new NotImplementedException(),
            TokenType.OpenBracket => throw new NotImplementedException(),
            TokenType.CloseBracket => throw new NotImplementedException(),
            TokenType.OpenScope => throw new NotImplementedException(),
            TokenType.CloseScope => throw new NotImplementedException(),
            TokenType.Dot => throw new NotImplementedException(),
            TokenType.Comma => throw new NotImplementedException(),
            TokenType.Addition => throw new NotImplementedException(),
            TokenType.Subtraction => throw new NotImplementedException(),
            TokenType.Multiplication => throw new NotImplementedException(),
            TokenType.Division => throw new NotImplementedException(),
            TokenType.Modulo => throw new NotImplementedException(),
            TokenType.LessThan => throw new NotImplementedException(),
            TokenType.GreaterThan => throw new NotImplementedException(),
            TokenType.LessThanEqual => throw new NotImplementedException(),
            TokenType.GreaterThanEqual => throw new NotImplementedException(),
            TokenType.EqualEqual => throw new NotImplementedException(),
            TokenType.NotEqual => throw new NotImplementedException(),
            TokenType.LogicalAnd => throw new NotImplementedException(),
            TokenType.LogicalOr => throw new NotImplementedException(),
            TokenType.LogicalNot => throw new NotImplementedException(),
            TokenType.AdditionAssignment => ParseVariableAssignmentShorthand(identifier, ParseBinaryExpression()),
            TokenType.SubtractionAssignment => throw new NotImplementedException(),
            TokenType.MultiplicationAssignment => throw new NotImplementedException(),
            TokenType.DivisionAssignment => throw new NotImplementedException(),
            TokenType.ModuloAssignment => throw new NotImplementedException(),
            TokenType.Decrement => throw new NotImplementedException(),
            TokenType.BitwiseComplement => throw new NotImplementedException(),
            TokenType.BitwiseAnd => throw new NotImplementedException(),
            TokenType.BitwiseOr => throw new NotImplementedException(),
            TokenType.BitwiseXOr => throw new NotImplementedException(),
            TokenType.BitwiseShiftLeft => throw new NotImplementedException(),
            TokenType.BitwiseShiftRight => throw new NotImplementedException(),
            TokenType.Semicolon => throw new NotImplementedException(),
            TokenType.ExternKeyword => throw new NotImplementedException(),
            TokenType.IfKeyword => throw new NotImplementedException(),
            TokenType.ElseKeyword => throw new NotImplementedException(),
            TokenType.WhileKeyword => throw new NotImplementedException(),
            TokenType.AutoKeyword => throw new NotImplementedException(),
            TokenType.SwitchKeyword => throw new NotImplementedException(),
            TokenType.CaseKeyword => throw new NotImplementedException(),
            TokenType.BreakKeyword => throw new NotImplementedException(),
            TokenType.ArrayIndexing => throw new NotImplementedException(),
            _ => throw new ParserException($"{data.GetFileLocation(previousTokenRange.End)} Unexpected token in {nameof(ParseIdentifierStatement)} of type {Peek()}"),
        };
    }

    private FunctionCall ParseFunctionCall(Token identifier, List<Expression> parameters)
    {
        while (!Peek(TokenType.CloseParenthesis))
        {
            parameters.Add(ParseParameter());
            if (Peek(TokenType.Comma))
            {
                _ = Eat(TokenType.Comma);
            }
        }
        _ = Eat(TokenType.CloseParenthesis);
        _ = Eat(TokenType.Semicolon);

        Symbol symbol = symbols.GetOrAdd(identifier, SymbolKind.Load);

        return new FunctionCall(symbol, parameters.ToArray())
        {
            Range = identifier.Range,
        };
    }

    private Expression ParseParameter()
    {
        return Peek() switch
        {
            TokenType.StringLiteral => ParseString(),
            TokenType.IntegerLiteral => ParseInteger(),
            TokenType.Identifier => ParseIdentifier(),
            TokenType.Eof => throw new NotImplementedException(),
            TokenType.Garbage => throw new NotImplementedException(),
            TokenType.None => throw new NotImplementedException(),
            TokenType.Comment => throw new NotImplementedException(),
            TokenType.FloatLiteral => throw new NotImplementedException(),
            TokenType.CharLiteral => throw new NotImplementedException(),
            TokenType.OpenParenthesis => throw new NotImplementedException(),
            TokenType.CloseParenthesis => throw new NotImplementedException(),
            TokenType.OpenBracket => throw new NotImplementedException(),
            TokenType.CloseBracket => throw new NotImplementedException(),
            TokenType.OpenScope => throw new NotImplementedException(),
            TokenType.CloseScope => throw new NotImplementedException(),
            TokenType.Dot => throw new NotImplementedException(),
            TokenType.Comma => throw new NotImplementedException(),
            TokenType.Addition => throw new NotImplementedException(),
            TokenType.Subtraction => throw new NotImplementedException(),
            TokenType.Multiplication => throw new NotImplementedException(),
            TokenType.Division => throw new NotImplementedException(),
            TokenType.Modulo => throw new NotImplementedException(),
            TokenType.LessThan => throw new NotImplementedException(),
            TokenType.GreaterThan => throw new NotImplementedException(),
            TokenType.LessThanEqual => throw new NotImplementedException(),
            TokenType.GreaterThanEqual => throw new NotImplementedException(),
            TokenType.EqualEqual => throw new NotImplementedException(),
            TokenType.NotEqual => throw new NotImplementedException(),
            TokenType.LogicalAnd => throw new NotImplementedException(),
            TokenType.LogicalOr => throw new NotImplementedException(),
            TokenType.LogicalNot => throw new NotImplementedException(),
            TokenType.Assignment => throw new NotImplementedException(),
            TokenType.AdditionAssignment => throw new NotImplementedException(),
            TokenType.SubtractionAssignment => throw new NotImplementedException(),
            TokenType.MultiplicationAssignment => throw new NotImplementedException(),
            TokenType.DivisionAssignment => throw new NotImplementedException(),
            TokenType.ModuloAssignment => throw new NotImplementedException(),
            TokenType.Increment => throw new NotImplementedException(),
            TokenType.Decrement => throw new NotImplementedException(),
            TokenType.BitwiseComplement => throw new NotImplementedException(),
            TokenType.BitwiseAnd => throw new NotImplementedException(),
            TokenType.BitwiseOr => throw new NotImplementedException(),
            TokenType.BitwiseXOr => throw new NotImplementedException(),
            TokenType.BitwiseShiftLeft => throw new NotImplementedException(),
            TokenType.BitwiseShiftRight => throw new NotImplementedException(),
            TokenType.Semicolon => throw new NotImplementedException(),
            TokenType.ExternKeyword => throw new NotImplementedException(),
            TokenType.IfKeyword => throw new NotImplementedException(),
            TokenType.ElseKeyword => throw new NotImplementedException(),
            TokenType.WhileKeyword => throw new NotImplementedException(),
            TokenType.AutoKeyword => throw new NotImplementedException(),
            TokenType.SwitchKeyword => throw new NotImplementedException(),
            TokenType.CaseKeyword => throw new NotImplementedException(),
            TokenType.BreakKeyword => throw new NotImplementedException(),
            TokenType.ArrayIndexing => throw new NotImplementedException(),
            _ => throw new ParserException($"{data.GetFileLocation(previousTokenRange.End)} ParseExpression: {Peek()}"),
        };
    }

    private VariableDeclaration ParseVariableAssignment(Token identifier)
    {
        Expression value = ParseBinaryExpression();
        _ = Eat(TokenType.Semicolon);

        Symbol symbol = symbols.GetOrAdd(identifier, SymbolKind.Assign);

        return new VariableDeclaration(symbol, value)
        {
            Range = identifier.Range,
        };
    }

    private VariableDeclaration ParseVariableAssignmentShorthand(Token identifier, Expression shorthandValue)
    {
        Symbol symbol = symbols.GetOrAdd(identifier, SymbolKind.Assign);

        Expression value = new BinaryExpression(TokenType.Addition, new Variable(symbol), shorthandValue);
        _ = Eat(TokenType.Semicolon);

        return new VariableDeclaration(symbol, value)
        {
            Range = identifier.Range,
        };
    }
}
