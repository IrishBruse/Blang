namespace IBlang;

using System.Diagnostics;

using IBlang.Data;

public class Parser
{
    private readonly Tokens tokens;

    private readonly bool debug;

    public Parser(Tokens tokens, bool debug = false)
    {
        this.debug = debug;
        this.tokens = tokens;
    }

    public FileAst Parse()
    {
        List<FunctionDecleration> functions = new();

        while (tokens.Peek.Type != TokenType.Eof)
        {
            switch (tokens.Peek.Type)
            {
                case TokenType.Keyword_Func: functions.Add(ParseFunctionDecleration()); break;
                case TokenType.Comment: tokens.EatToken(TokenType.Comment); break;
                case TokenType.Eof: return new FileAst(functions.ToArray());

                default:
                tokens.AddError(new ParseError($"Unexpected token {tokens.Peek.Type}: {tokens.Peek.Value}", tokens.Peek.Span, new StackTrace(true)));
                tokens.Skip();
                break;
            }
        }

        return new FileAst(functions.ToArray());
    }

    private FunctionDecleration ParseFunctionDecleration()
    {
        tokens.EatToken(TokenType.Keyword_Func);

        string name = tokens.EatToken(TokenType.Identifier);

        ParameterDefinition[] parameters = ParseParameterDefinitions();
        BlockBody statements = ParseBlock();

        return new FunctionDecleration(name, parameters, statements);
    }

    private ParameterDefinition[] ParseParameterDefinitions()
    {
        tokens.EatToken(TokenType.OpenParenthesis);

        List<ParameterDefinition> parameters = new();

        while (tokens.Peek.Type != TokenType.CloseParenthesis)
        {
            string type = tokens.EatIdentifier();
            string identifier = tokens.EatIdentifier();
            parameters.Add(new ParameterDefinition(type, identifier));

            tokens.EatToken(TokenType.Identifier); // Name

            if (tokens.TryEatToken(TokenType.Comma))
            {
                continue;
            }
            else
            {
                break;
            }
        }

        tokens.EatToken(TokenType.CloseParenthesis);

        return parameters.ToArray();
    }

    private BlockBody ParseBlock()
    {
        tokens.EatToken(TokenType.OpenScope);

        List<Statement> statements = new();

        while (tokens.Peek.Type != TokenType.CloseScope)
        {
            statements.Add(ParseStatement());
        }

        tokens.EatToken(TokenType.CloseScope);

        return new(statements);
    }

    private Statement ParseStatement()
    {
        return tokens.Peek.Type switch
        {
            TokenType.Identifier => ParseIdentifierStatement(),
            TokenType.Keyword_If => ParseIfStatement(),
            TokenType.Keyword_Return => ParseReturn(),
            _ => tokens.Error($"Unexpected token {tokens.Peek.Type}: {tokens.Peek.Value} in ParseStatement"),
        };
    }

    private Statement ParseIdentifierStatement()
    {
        string identifier = tokens.EatToken(TokenType.Identifier);

        return tokens.Peek.Type switch
        {
            TokenType.OpenParenthesis => ParseFunctionCall(identifier),
            TokenType.Assignment => ParseAssignmentStatement(identifier),
            _ => tokens.Error($"Unexpected token {tokens.Peek.Type}: {tokens.Peek.Value} in " + nameof(ParseIdentifierStatement)),
        };
    }

    private AssignmentStatement ParseAssignmentStatement(string identifier)
    {
        tokens.EatKeyword(TokenType.Assignment);

        return new(identifier, ParseExpression());
    }

    private ReturnStatement ParseReturn()
    {
        tokens.EatKeyword(TokenType.Keyword_Return);

        Expression expression = ParseExpression();
        return new ReturnStatement(expression);
    }

    private FunctionCallExpression ParseFunctionCall(string identifier)
    {
        List<Expression> args = new();

        tokens.EatToken(TokenType.OpenParenthesis);
        while (tokens.Peek.Type != TokenType.CloseParenthesis)
        {
            args.Add(ParseExpression());
            tokens.TryEatToken(TokenType.Comma);
        }

        tokens.EatToken(TokenType.CloseParenthesis);

        return new FunctionCallExpression(identifier, args.ToArray());
    }

    private Expression ParseExpression()
    {
        return tokens.Peek.Type switch
        {
            TokenType.Identifier => ParseIdentifierExpression(),
            TokenType.StringLiteral => ParseStringLiteral(),
            TokenType.IntegerLiteral => ParseIntegerLiteral(),
            _ => tokens.Error($"{tokens.Peek.Type} is not a valid parameter type in " + nameof(ParseExpression)),
        };
    }

    private Expression ParseIdentifierExpression()
    {
        string identifier = tokens.EatToken(TokenType.Identifier);

        return tokens.Peek.Type switch
        {
            TokenType.OpenParenthesis => ParseFunctionCall(identifier),
            TokenType.CloseParenthesis => new Identifier(identifier), // variable identifier
            _ => tokens.Error($"{tokens.Peek.Type} is not a valid parameter type in " + nameof(ParseExpression)),
        };
    }

    /// <summary> if <BinaryExpression> { } </summary>
    private IfStatement ParseIfStatement()
    {
        tokens.EatToken(TokenType.Keyword_If);
        BooleanExpression condition = ParseBooleanExpression();
        BlockBody body = ParseBlock();
        BlockBody? elseBody = null;

        if (tokens.TryEatToken(TokenType.Keyword_Else))
        {
            elseBody = ParseBlock();
        }

        return new IfStatement(condition, body, elseBody);
    }

    private BinaryExpression ParseBinaryExpression()
    {
        Expression left = ParseExpression();

        switch (tokens.Peek.Type)
        {
            case TokenType.Addition:
            tokens.EatToken(TokenType.Addition);
            break;

            case TokenType.Subtraction:
            tokens.EatToken(TokenType.Subtraction);
            break;

            case TokenType.Multiplication:
            tokens.EatToken(TokenType.Multiplication);
            break;

            case TokenType.Division:
            tokens.EatToken(TokenType.Division);
            break;

            case TokenType.EqualEqual:
            tokens.EatToken(TokenType.EqualEqual);
            break;

            default:
            tokens.AddError(new ParseError($"Unexpected token {tokens.Peek.Type}: {tokens.Peek.Value} in " + nameof(ParseBinaryExpression), tokens.Peek.Span, new StackTrace(true)));
            break;
        }

        Expression right = ParseExpression();

        return new BinaryExpression(left, right);
    }

    private BooleanExpression ParseBooleanExpression()
    {
        Expression left = ParseExpression();

        switch (tokens.Peek.Type)
        {
            case TokenType.EqualEqual:
            tokens.EatToken(TokenType.EqualEqual);
            break;

            default:
            tokens.AddError(new ParseError($"Unexpected token {tokens.Peek.Type}: {tokens.Peek.Value} in " + nameof(ParseBooleanExpression), tokens.Peek.Span, new StackTrace(true)));
            break;
        }

        Expression right = ParseExpression();

        return new BooleanExpression(left, right);
    }

    private StringLiteral ParseStringLiteral()
    {
        string token = tokens.EatToken(TokenType.StringLiteral);
        return new StringLiteral(token);
    }

    private IntegerLiteral ParseIntegerLiteral()
    {
        int token = tokens.EatNumber(TokenType.IntegerLiteral);
        return new IntegerLiteral(token);
    }

    public void PrintErrors()
    {
        tokens.ListErrors();
    }
}
