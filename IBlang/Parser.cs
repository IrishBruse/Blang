namespace IBlang;
public class Parser
{
    private readonly Token[] tokens;
    private int currentTokenIndex;

    private Token Peek => tokens[currentTokenIndex];
    private TokenType PeekType => Peek.Type;

    public Parser(Token[] tokens)
    {
        this.tokens = tokens;
    }

    public FileAst Parse()
    {
        List<FunctionDecleration> functions = new();
        while (true)
        {
            switch (PeekType)
            {
                case TokenType.Keyword_Func: functions.Add(ParseFunctionDecleration()); break;
                case TokenType.Eof: return new FileAst(functions.ToArray());

                default: throw new ParseException($"Unexpected token {Peek}");
            }
        }
    }

    private FunctionDecleration ParseFunctionDecleration()
    {
        EatToken(TokenType.Keyword_Func);
        string name = EatToken(TokenType.Identifier);
        Parameter[] parameters = ParseParameterDefinitions();

        EatToken(TokenType.OpenScope);
        Statement[] statements = ParseStatements();
        EatToken(TokenType.CloseScope);

        return new FunctionDecleration(name, parameters, statements);
    }

    private Parameter[] ParseParameterDefinitions()
    {
        EatToken(TokenType.OpenParenthesis);

        List<Parameter> parameters = new();

        while (PeekType != TokenType.CloseParenthesis)
        {
            parameters.Add(new Parameter(EatIdentifier(), EatIdentifier()));

            EatToken(TokenType.Identifier); // Name

            if (TryEatToken(TokenType.Comma))
            {
                continue;
            }
            else
            {
                break;
            }
        }

        EatToken(TokenType.CloseParenthesis);

        return parameters.ToArray();
    }

    private Statement[] ParseStatements()
    {
        List<Statement> statements = new();

        while (PeekType != TokenType.CloseScope)
        {
            if (PeekType == TokenType.Identifier)
            {
                switch (Peek.Value)
                {
                    case "if":
                    statements.Add(ParseIfStatement());
                    break;

                    default:
                    statements.Add(ParseUnaryExpression());
                    break;
                }
            }
            else
            {
                throw new ParseException($"Unexpected token {Peek}");
            }
        }

        return statements.ToArray();
    }

    private Statement ParseUnaryExpression()
    {
        switch (PeekType)
        {
            case TokenType.Identifier:
            string name = EatToken(TokenType.Identifier);

            List<INode> args = new();

            if (TryEatToken(TokenType.OpenParenthesis))
            {
                args.Add(ParseUnaryExpression());
                EatToken(TokenType.CloseParenthesis);
            }
            else if (TryEatToken(TokenType.Assignment))
            {
                throw new NotImplementedException();
            }
            return new FunctionCall(name, args.ToArray());

            case TokenType.StringLiteral:
            string value = EatToken(TokenType.StringLiteral);
            return new StringLiteral(value);

            default: throw new ParseException($"Unexpected token {Peek}");
        }
    }

    private IfStatement ParseIfStatement()
    {
        EatToken(TokenType.Keyword_If);
        ParseBinaryExpression();
        EatToken(TokenType.OpenScope);
        Statement[] statements = ParseStatements();
        EatToken(TokenType.CloseScope);

        return new IfStatement(Array.Empty<Parameter>(), statements);
    }

    private BinaryExpression ParseBinaryExpression()
    {
        INode left = ParseUnaryExpression();

        switch (PeekType)
        {
            case TokenType.Addition:
            EatToken(TokenType.Addition);
            break;

            case TokenType.Subtraction:
            EatToken(TokenType.Subtraction);
            break;

            case TokenType.Multiplication:
            EatToken(TokenType.Multiplication);
            break;

            case TokenType.Division:
            EatToken(TokenType.Division);
            break;

            default: throw new ParseException($"Unexpected token {Peek}");
        }

        INode right = ParseUnaryExpression();

        return new BinaryExpression(left, right);
    }

    private string EatToken(TokenType type)
    {
        Token p = Peek;

        if (p.Type != type)
        {
            throw new ParseException($"Expected token of type {type} but got {Peek.Type} with value '{Peek.Value}'");
        }

        currentTokenIndex++;

        return p.Value;
    }

    private bool TryEatToken(TokenType type)
    {
        Token p = Peek;

        if (p.Type != type)
        {
            return false;
        }
        else
        {
            currentTokenIndex++;
            return true;
        }
    }

    private string EatIdentifier()
    {
        Token p = Peek;

        if (p.Type != TokenType.Identifier)
        {
            throw new ParseException($"Expected identifier but got {Peek.Type} with value '{Peek.Value}'");
        }

        currentTokenIndex++;

        return p.Value;
    }
}
