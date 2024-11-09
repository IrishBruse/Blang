namespace IBlang;

using System.Diagnostics;

using IBlang.Data;

public class Parser(Project compilation)
{
    readonly Project compilation = compilation;

    public FileAst Parse()
    {
        List<FunctionDecleration> functions = [];

        string file = compilation.Peek.Span.File;

        while (compilation.Peek.Type != TokenType.Eof)
        {
            switch (compilation.Peek.Type)
            {
                case TokenType.Keyword_Func:
                functions.Add(ParseFunctionDecleration());
                break;

                case TokenType.Comment:
                _ = compilation.EatToken(TokenType.Comment);
                break;

                case TokenType.Eof: break;

                default:
                compilation.AddError(new ParseError($"Unexpected token {compilation.Peek.Type}: {compilation.Peek.Value}", compilation.Peek.Span, new StackTrace(true)));
                compilation.Skip();
                break;
            }
        }

        if (!functions.Any(decl => decl.Name == "Main"))
        {
            compilation.AddError(new ParseError("Entry Main() not found", new Span(file, 0, 0), new StackTrace(true)));
        }

        return new FileAst(functions.ToArray(), file);
    }

    FunctionDecleration ParseFunctionDecleration()
    {
        _ = compilation.EatToken(TokenType.Keyword_Func);

        string name = compilation.EatToken(TokenType.Identifier);

        ParameterDefinition[] parameters = ParseParameterDefinitions();

        Token returnType = compilation.Peek;

        if (returnType.Type == TokenType.Identifier)
        {
            _ = compilation.EatToken(TokenType.Identifier);
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
        _ = compilation.EatToken(TokenType.OpenParenthesis);

        List<ParameterDefinition> parameters = [];

        while (compilation.Peek.Type != TokenType.CloseParenthesis)
        {
            string type = compilation.EatIdentifier();
            string identifier = compilation.EatIdentifier();
            parameters.Add(new ParameterDefinition(type, identifier));

            // tokens.EatToken(TokenType.Identifier); // Name

            if (compilation.TryEatToken(TokenType.Comma))
            {
                continue;
            }
            else
            {
                break;
            }
        }

        _ = compilation.EatToken(TokenType.CloseParenthesis);

        return parameters.ToArray();
    }

    BlockBody ParseBlock()
    {
        _ = compilation.EatToken(TokenType.OpenScope);

        List<Statement> statements = [];

        while (compilation.Peek.Type != TokenType.CloseScope)
        {
            statements.Add(ParseStatement());
        }

        _ = compilation.EatToken(TokenType.CloseScope);

        return new(statements.ToArray());
    }

    Statement ParseStatement()
    {
        return compilation.Peek.Type switch
        {
            TokenType.Identifier => ParseIdentifierStatement(),
            TokenType.Keyword_If => ParseIfStatement(),
            TokenType.Keyword_Return => ParseReturn(),
            _ => compilation.Error($"Unexpected token {compilation.Peek.Type}: {compilation.Peek.Value} in ParseStatement"),
        };
    }

    Statement ParseIdentifierStatement()
    {
        string identifier = compilation.EatToken(TokenType.Identifier);

        return compilation.Peek.Type switch
        {
            TokenType.OpenParenthesis => ParseFunctionCallStatement(identifier),
            TokenType.Assignment => ParseAssignmentStatement(identifier),
            _ => compilation.Error($"Unexpected token {compilation.Peek.Type}: {compilation.Peek.Value} in " + nameof(ParseIdentifierStatement)),
        };
    }

    Expression ParseIdentifierExpression()
    {
        string identifier = compilation.EatToken(TokenType.Identifier);

        return compilation.Peek.Type switch
        {
            TokenType.OpenParenthesis => ParseFunctionCall(identifier),
            _ => new Identifier(identifier),
        };
    }

    AssignmentStatement ParseAssignmentStatement(string identifier)
    {
        compilation.EatKeyword(TokenType.Assignment);

        return new(identifier, ParseExpression());
    }

    ReturnStatement ParseReturn()
    {
        compilation.EatKeyword(TokenType.Keyword_Return);

        Expression expression = ParseExpression();
        return new ReturnStatement(expression);
    }

    FunctionCallExpression ParseFunctionCall(string identifier)
    {
        List<Expression> args = [];

        _ = compilation.EatToken(TokenType.OpenParenthesis);
        while (compilation.Peek.Type != TokenType.CloseParenthesis)
        {
            args.Add(ParseExpression());
            _ = compilation.TryEatToken(TokenType.Comma);
        }

        _ = compilation.EatToken(TokenType.CloseParenthesis);

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
            if (compilation.Eof() || !compilation.Peek.IsBinaryOperator() || precedence[compilation.Peek.Type] < minPrecedence)
            {
                return lhs;
            }

            Debug.Assert(compilation.Peek.IsBinaryOperator());

            int nextPrecedence = precedence[compilation.Peek.Type] + 1;

            Token operation = compilation.Peek;
            compilation.Skip();

            Expression rhs = ParseExpression(nextPrecedence);

            lhs = new BinaryExpression(operation, lhs, rhs);
        }
    }

    Expression ParseAtom()
    {
        if (compilation.Peek.Type == TokenType.OpenParenthesis)
        {
            Expression expression = ParseExpression();
            _ = compilation.EatToken(TokenType.CloseParenthesis, "Mismatched parenthesis, expected ')'");
            return expression;
        }
        else if (compilation.Peek.Type == TokenType.Eof)
        {
            return compilation.Error($"Unexpected End Of File reached in " + nameof(ParseAtom));
        }
        else if (compilation.Peek.IsBinaryOperator())
        {
            return compilation.Error($"{compilation.Peek.Type} is not an Atom value in expression " + nameof(ParseAtom));
        }
        else
        {
            return compilation.Peek.Type switch
            {
                TokenType.Identifier => ParseIdentifierExpression(),
                TokenType.StringLiteral => ParseStringLiteral(),
                TokenType.IntegerLiteral => ParseIntegerLiteral(),
                TokenType.FloatLiteral => ParseFloatLiteral(),
                _ => compilation.Error($"{compilation.Peek.Type} is not a valid value in expression " + nameof(ParseAtom)),
            };
        }
    }

    /// <summary> if <BinaryExpression> { } </summary>
    IfStatement ParseIfStatement()
    {
        _ = compilation.EatToken(TokenType.Keyword_If);
        BooleanExpression condition = ParseBooleanExpression();
        BlockBody body = ParseBlock();
        BlockBody? elseBody = null;

        if (compilation.TryEatToken(TokenType.Keyword_Else))
        {
            elseBody = ParseBlock();
        }

        return new IfStatement(condition, body, elseBody);
    }

    BinaryExpression ParseBinaryExpression()
    {
        Expression left = ParseExpression();

        Token operation = compilation.Peek;

        switch (compilation.Peek.Type)
        {
            case TokenType.Addition:
            _ = compilation.EatToken(TokenType.Addition);
            break;

            case TokenType.Subtraction:
            _ = compilation.EatToken(TokenType.Subtraction);
            break;

            case TokenType.Multiplication:
            _ = compilation.EatToken(TokenType.Multiplication);
            break;

            case TokenType.Division:
            _ = compilation.EatToken(TokenType.Division);
            break;

            case TokenType.EqualEqual:
            _ = compilation.EatToken(TokenType.EqualEqual);
            break;

            default:
            compilation.AddError(new ParseError($"Unexpected token {compilation.Peek.Type}: {compilation.Peek.Value} in " + nameof(ParseBinaryExpression), compilation.Peek.Span, new StackTrace(true)));
            break;
        }

        Expression right = ParseExpression();

        return new BinaryExpression(operation, left, right);
    }

    BooleanExpression ParseBooleanExpression()
    {
        Expression left = ParseExpression();

        Token operation = compilation.Peek;

        if (operation.Type == TokenType.EqualEqual)
        {
            _ = compilation.EatToken(TokenType.EqualEqual);
        }
        else if (operation.Type == TokenType.LessThanEqual)
        {
            _ = compilation.EatToken(TokenType.LessThanEqual);
        }
        else
        {
            compilation.AddError(new ParseError($"Unexpected token {compilation.Peek.Type}: {compilation.Peek.Value} in " + nameof(ParseBooleanExpression), compilation.Peek.Span, new StackTrace(true)));
        }

        Expression right = ParseExpression();

        return new BooleanExpression(operation, left, right);
    }

    Identifier ParseIdentifier()
    {
        string token = compilation.EatToken(TokenType.Identifier);
        return new Identifier(token);
    }

    StringLiteral ParseStringLiteral()
    {
        string token = compilation.EatToken(TokenType.StringLiteral);
        compilation.AddString(token);
        return new StringLiteral(token);
    }

    IntegerLiteral ParseIntegerLiteral()
    {
        int token = compilation.EatNumber(TokenType.IntegerLiteral);
        return new IntegerLiteral(token);
    }

    FloatLiteral ParseFloatLiteral()
    {
        int token = compilation.EatNumber(TokenType.FloatLiteral);
        return new FloatLiteral(token);
    }
}
