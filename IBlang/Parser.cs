namespace IBlang;

using System.Diagnostics;

using IBlang.Data;

public class Parser(Tokens tokens)
{
    readonly Tokens tokens = tokens;

    public FileAst Parse()
    {
        List<FunctionDecleration> functions = [];

        string file = tokens.Peek.Span.File;

        while (tokens.Peek.Type != TokenType.Eof)
        {
            switch (tokens.Peek.Type)
            {
                case TokenType.Keyword_Func:
                functions.Add(ParseFunctionDecleration());
                break;

                case TokenType.Comment:
                _ = tokens.EatToken(TokenType.Comment);
                break;

                case TokenType.Eof: return new FileAst(functions.ToArray(), file);

                default:
                tokens.AddError(new ParseError($"Unexpected token {tokens.Peek.Type}: {tokens.Peek.Value}", tokens.Peek.Span, null));
                tokens.Skip();
                break;
            }
        }
        return new FileAst(functions.ToArray(), file);
    }

    FunctionDecleration ParseFunctionDecleration()
    {
        _ = tokens.EatToken(TokenType.Keyword_Func);

        string name = tokens.EatToken(TokenType.Identifier);

        ParameterDefinition[] parameters = ParseParameterDefinitions();

        Token returnType = tokens.Peek;

        if (returnType.Type == TokenType.Identifier)
        {
            _ = tokens.EatToken(TokenType.Identifier);
        }
        else
        {
            returnType = new Token("void", TokenType.Identifier, returnType.Span);
        }

        BlockBody statements = ParseBlock();

        return new FunctionDecleration(name, returnType, parameters, statements);
    }

    ParameterDefinition[] ParseParameterDefinitions()
    {
        _ = tokens.EatToken(TokenType.OpenParenthesis);

        List<ParameterDefinition> parameters = [];

        while (tokens.Peek.Type != TokenType.CloseParenthesis)
        {
            string type = tokens.EatIdentifier();
            string identifier = tokens.EatIdentifier();
            parameters.Add(new ParameterDefinition(type, identifier));

            // tokens.EatToken(TokenType.Identifier); // Name

            if (tokens.TryEatToken(TokenType.Comma))
            {
                continue;
            }
            else
            {
                break;
            }
        }

        _ = tokens.EatToken(TokenType.CloseParenthesis);

        return parameters.ToArray();
    }

    BlockBody ParseBlock()
    {
        _ = tokens.EatToken(TokenType.OpenScope);

        List<Statement> statements = [];

        while (tokens.Peek.Type != TokenType.CloseScope)
        {
            statements.Add(ParseStatement());
        }

        _ = tokens.EatToken(TokenType.CloseScope);

        return new(statements.ToArray());
    }

    Statement ParseStatement()
    {
        return tokens.Peek.Type switch
        {
            TokenType.Identifier => ParseIdentifierStatement(),
            TokenType.Keyword_If => ParseIfStatement(),
            TokenType.Keyword_Return => ParseReturn(),
            _ => tokens.Error($"Unexpected token {tokens.Peek.Type}: {tokens.Peek.Value} in ParseStatement"),
        };
    }

    Statement ParseIdentifierStatement()
    {
        string identifier = tokens.EatToken(TokenType.Identifier);

        return tokens.Peek.Type switch
        {
            TokenType.OpenParenthesis => ParseFunctionCallStatement(identifier),
            TokenType.Assignment => ParseAssignmentStatement(identifier),
            _ => tokens.Error($"Unexpected token {tokens.Peek.Type}: {tokens.Peek.Value} in " + nameof(ParseIdentifierStatement)),
        };
    }

    Expression ParseIdentifierExpression()
    {
        string identifier = tokens.EatToken(TokenType.Identifier);

        return tokens.Peek.Type switch
        {
            TokenType.OpenParenthesis => ParseFunctionCall(identifier),
            _ => new Identifier(identifier),
        };
    }

    AssignmentStatement ParseAssignmentStatement(string identifier)
    {
        tokens.EatKeyword(TokenType.Assignment);

        return new(identifier, ParseExpression());
    }

    ReturnStatement ParseReturn()
    {
        tokens.EatKeyword(TokenType.Keyword_Return);

        Expression expression = ParseExpression();
        return new ReturnStatement(expression);
    }

    FunctionCallExpression ParseFunctionCall(string identifier)
    {
        List<Expression> args = [];

        _ = tokens.EatToken(TokenType.OpenParenthesis);
        while (tokens.Peek.Type != TokenType.CloseParenthesis)
        {
            args.Add(ParseExpression());
            _ = tokens.TryEatToken(TokenType.Comma);
        }

        _ = tokens.EatToken(TokenType.CloseParenthesis);

        return new FunctionCallExpression(identifier, args.ToArray());
    }

    FunctionCallStatement ParseFunctionCallStatement(string identifier)
    {
        FunctionCallExpression functionCall = ParseFunctionCall(identifier);

        return new FunctionCallStatement(functionCall.Name, functionCall.Args);
    }

    static Dictionary<TokenType, int> precedence = new()
    {
        { TokenType.Addition, 1 },
        { TokenType.Subtraction, 1 },
        { TokenType.Multiplication, 2 },
        { TokenType.Division, 2 },
        { TokenType.Modulo, 2 },
        { TokenType.Identifier, 3 }, // Function call
    };

    Expression ParseExpression(int minPrecedence = 1)
    {
        Expression lhs = ParseAtom();

        while (true)
        {
            if (tokens.Eof() || !tokens.Peek.IsBinaryOperator() || precedence[tokens.Peek.Type] < minPrecedence)
            {
                return lhs;
            }

            Debug.Assert(tokens.Peek.IsBinaryOperator());

            int nextPrecedence = precedence[tokens.Peek.Type] + 1;

            Token operation = tokens.Peek;
            tokens.Skip();

            Expression rhs = ParseExpression(nextPrecedence);

            lhs = new BinaryExpression(operation, lhs, rhs);
        }
    }

    Expression ParseAtom()
    {
        if (tokens.Peek.Type == TokenType.OpenParenthesis)
        {
            Expression expression = ParseExpression();
            _ = tokens.EatToken(TokenType.CloseParenthesis, "Mismatched parenthesis, expected ')'");
            return expression;
        }
        else if (tokens.Peek.Type == TokenType.Eof)
        {
            return tokens.Error($"Unexpected End Of File reached in " + nameof(ParseAtom));
        }
        else if (tokens.Peek.IsBinaryOperator())
        {
            return tokens.Error($"{tokens.Peek.Type} is not an Atom value in expression " + nameof(ParseAtom));
        }
        else
        {
            return tokens.Peek.Type switch
            {
                TokenType.Identifier => ParseIdentifierExpression(),
                TokenType.StringLiteral => ParseStringLiteral(),
                TokenType.IntegerLiteral => ParseIntegerLiteral(),
                TokenType.FloatLiteral => ParseFloatLiteral(),
                _ => tokens.Error($"{tokens.Peek.Type} is not a valid value in expression " + nameof(ParseAtom)),
            };
        }
    }

    /// <summary> if <BinaryExpression> { } </summary>
    IfStatement ParseIfStatement()
    {
        _ = tokens.EatToken(TokenType.Keyword_If);
        BooleanExpression condition = ParseBooleanExpression();
        BlockBody body = ParseBlock();
        BlockBody? elseBody = null;

        if (tokens.TryEatToken(TokenType.Keyword_Else))
        {
            elseBody = ParseBlock();
        }

        return new IfStatement(condition, body, elseBody);
    }

    BinaryExpression ParseBinaryExpression()
    {
        Expression left = ParseExpression();

        Token operation = tokens.Peek;

        switch (tokens.Peek.Type)
        {
            case TokenType.Addition:
            _ = tokens.EatToken(TokenType.Addition);
            break;

            case TokenType.Subtraction:
            _ = tokens.EatToken(TokenType.Subtraction);
            break;

            case TokenType.Multiplication:
            _ = tokens.EatToken(TokenType.Multiplication);
            break;

            case TokenType.Division:
            _ = tokens.EatToken(TokenType.Division);
            break;

            case TokenType.EqualEqual:
            _ = tokens.EatToken(TokenType.EqualEqual);
            break;

            default:
            tokens.AddError(new ParseError($"Unexpected token {tokens.Peek.Type}: {tokens.Peek.Value} in " + nameof(ParseBinaryExpression), tokens.Peek.Span, new StackTrace(true)));
            break;
        }

        Expression right = ParseExpression();

        return new BinaryExpression(operation, left, right);
    }

    BooleanExpression ParseBooleanExpression()
    {
        Expression left = ParseExpression();

        Token operation = tokens.Peek;

        if (operation.Type == TokenType.EqualEqual)
        {
            _ = tokens.EatToken(TokenType.EqualEqual);
        }
        else if (operation.Type == TokenType.LessThanEqual)
        {
            _ = tokens.EatToken(TokenType.LessThanEqual);
        }
        else
        {
            tokens.AddError(new ParseError($"Unexpected token {tokens.Peek.Type}: {tokens.Peek.Value} in " + nameof(ParseBooleanExpression), tokens.Peek.Span, new StackTrace(true)));
        }

        Expression right = ParseExpression();

        return new BooleanExpression(operation, left, right);
    }

    Identifier ParseIdentifier()
    {
        string token = tokens.EatToken(TokenType.Identifier);
        return new Identifier(token);
    }

    StringLiteral ParseStringLiteral()
    {
        string token = tokens.EatToken(TokenType.StringLiteral);
        return new StringLiteral(token);
    }

    IntegerLiteral ParseIntegerLiteral()
    {
        int token = tokens.EatNumber(TokenType.IntegerLiteral);
        return new IntegerLiteral(token);
    }

    FloatLiteral ParseFloatLiteral()
    {
        int token = tokens.EatNumber(TokenType.FloatLiteral);
        return new FloatLiteral(token);
    }
}
