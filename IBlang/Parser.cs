namespace IBlang;

using System.Diagnostics;

using IBlang.Data;

public class Parser
{
    readonly Tokens tokens;

    readonly bool debug;

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
            if (tokens.Peek.Type == TokenType.Keyword_Func)
            {
                functions.Add(ParseFunctionDecleration());
            }
            else if (tokens.Peek.Type == TokenType.Comment)
            {
                tokens.EatToken(TokenType.Comment);
            }
            else if (tokens.Peek.Type == TokenType.Eof)
            {
                return new FileAst(functions.ToArray());
            }
            else
            {
                tokens.AddError(new ParseError($"Unexpected token {tokens.Peek.Type}: {tokens.Peek.Value}", tokens.Peek.Span, new StackTrace(true)));
                tokens.Skip();
            }
        }
        return new FileAst(functions.ToArray());
    }

    FunctionDecleration ParseFunctionDecleration()
    {
        tokens.EatToken(TokenType.Keyword_Func);

        string name = tokens.EatToken(TokenType.Identifier);

        ParameterDefinition[] parameters = ParseParameterDefinitions();
        BlockBody statements = ParseBlock();

        return new FunctionDecleration(name, parameters, statements);
    }

    ParameterDefinition[] ParseParameterDefinitions()
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

    BlockBody ParseBlock()
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
            TokenType.OpenParenthesis => ParseFunctionCall(identifier),
            TokenType.Assignment => ParseAssignmentStatement(identifier),
            _ => tokens.Error($"Unexpected token {tokens.Peek.Type}: {tokens.Peek.Value} in " + nameof(ParseIdentifierStatement)),
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

    Expression ParseExpression()
    {
        return tokens.Peek.Type switch
        {
            TokenType.Identifier => ParseIdentifierExpression(),
            TokenType.StringLiteral => ParseStringLiteral(),
            TokenType.IntegerLiteral => ParseIntegerLiteral(),
            _ => tokens.Error($"{tokens.Peek.Type} is not a valid parameter type in " + nameof(ParseExpression)),
        };
    }

    Expression ParseIdentifierExpression()
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
    IfStatement ParseIfStatement()
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

    BinaryExpression ParseBinaryExpression()
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
            case TokenType.Eol:
            break;
            case TokenType.Eof:
            break;
            case TokenType.Garbage:
            break;
            case TokenType.Identifier:
            break;
            case TokenType.Comment:
            break;
            case TokenType.IntegerLiteral:
            break;
            case TokenType.FloatLiteral:
            break;
            case TokenType.StringLiteral:
            break;
            case TokenType.CharLiteral:
            break;
            case TokenType.OpenParenthesis:
            break;
            case TokenType.CloseParenthesis:
            break;
            case TokenType.OpenBracket:
            break;
            case TokenType.CloseBracket:
            break;
            case TokenType.OpenScope:
            break;
            case TokenType.CloseScope:
            break;
            case TokenType.Dot:
            break;
            case TokenType.Comma:
            break;
            case TokenType.Modulo:
            break;
            case TokenType.LessThan:
            break;
            case TokenType.GreaterThan:
            break;
            case TokenType.LessThanEqual:
            break;
            case TokenType.GreaterThanEqual:
            break;
            case TokenType.NotEqual:
            break;
            case TokenType.LogicalAnd:
            break;
            case TokenType.LogicalOr:
            break;
            case TokenType.LogicalNot:
            break;
            case TokenType.Assignment:
            break;
            case TokenType.AdditionAssignment:
            break;
            case TokenType.SubtractionAssignment:
            break;
            case TokenType.MultiplicationAssignment:
            break;
            case TokenType.DivisionAssignment:
            break;
            case TokenType.ModuloAssignment:
            break;
            case TokenType.BitwiseComplement:
            break;
            case TokenType.BitwiseAnd:
            break;
            case TokenType.BitwiseOr:
            break;
            case TokenType.BitwiseXOr:
            break;
            case TokenType.BitwiseShiftLeft:
            break;
            case TokenType.BitwiseShiftRight:
            break;
            case TokenType.Keyword_Func:
            break;
            case TokenType.Keyword_True:
            break;
            case TokenType.Keyword_False:
            break;
            case TokenType.Keyword_If:
            break;
            case TokenType.Keyword_Else:
            break;
            case TokenType.Keyword_Return:
            break;
            default:
            tokens.AddError(new ParseError($"Unexpected token {tokens.Peek.Type}: {tokens.Peek.Value} in " + nameof(ParseBinaryExpression), tokens.Peek.Span, new StackTrace(true)));
            break;
        }

        Expression right = ParseExpression();

        return new BinaryExpression(left, right);
    }

    BooleanExpression ParseBooleanExpression()
    {
        Expression left = ParseExpression();

        switch (tokens.Peek.Type)
        {
            case TokenType.EqualEqual:
            tokens.EatToken(TokenType.EqualEqual);
            break;
            case TokenType.Eol:
            break;
            case TokenType.Eof:
            break;
            case TokenType.Garbage:
            break;
            case TokenType.Identifier:
            break;
            case TokenType.Comment:
            break;
            case TokenType.IntegerLiteral:
            break;
            case TokenType.FloatLiteral:
            break;
            case TokenType.StringLiteral:
            break;
            case TokenType.CharLiteral:
            break;
            case TokenType.OpenParenthesis:
            break;
            case TokenType.CloseParenthesis:
            break;
            case TokenType.OpenBracket:
            break;
            case TokenType.CloseBracket:
            break;
            case TokenType.OpenScope:
            break;
            case TokenType.CloseScope:
            break;
            case TokenType.Dot:
            break;
            case TokenType.Comma:
            break;
            case TokenType.Addition:
            break;
            case TokenType.Subtraction:
            break;
            case TokenType.Multiplication:
            break;
            case TokenType.Division:
            break;
            case TokenType.Modulo:
            break;
            case TokenType.LessThan:
            break;
            case TokenType.GreaterThan:
            break;
            case TokenType.LessThanEqual:
            break;
            case TokenType.GreaterThanEqual:
            break;
            case TokenType.NotEqual:
            break;
            case TokenType.LogicalAnd:
            break;
            case TokenType.LogicalOr:
            break;
            case TokenType.LogicalNot:
            break;
            case TokenType.Assignment:
            break;
            case TokenType.AdditionAssignment:
            break;
            case TokenType.SubtractionAssignment:
            break;
            case TokenType.MultiplicationAssignment:
            break;
            case TokenType.DivisionAssignment:
            break;
            case TokenType.ModuloAssignment:
            break;
            case TokenType.BitwiseComplement:
            break;
            case TokenType.BitwiseAnd:
            break;
            case TokenType.BitwiseOr:
            break;
            case TokenType.BitwiseXOr:
            break;
            case TokenType.BitwiseShiftLeft:
            break;
            case TokenType.BitwiseShiftRight:
            break;
            case TokenType.Keyword_Func:
            break;
            case TokenType.Keyword_True:
            break;
            case TokenType.Keyword_False:
            break;
            case TokenType.Keyword_If:
            break;
            case TokenType.Keyword_Else:
            break;
            case TokenType.Keyword_Return:
            break;
            default:
            tokens.AddError(new ParseError($"Unexpected token {tokens.Peek.Type}: {tokens.Peek.Value} in " + nameof(ParseBooleanExpression), tokens.Peek.Span, new StackTrace(true)));
            break;
        }

        Expression right = ParseExpression();

        return new BooleanExpression(left, right);
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

    public void PrintErrors()
    {
        tokens.ListErrors();
    }
}
