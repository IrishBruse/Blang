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
        List<Statement> statements = new();

        EatToken(TokenType.Keyword_Func);
        EatToken(TokenType.Identifier);
        Parameter[] parameters = ParseParameterDefinitions();

        EatToken(TokenType.OpenScope);
        ParseStatements();
        EatToken(TokenType.CloseScope);

        return new FunctionDecleration(parameters, statements.ToArray());
    }

    private Parameter[] ParseParameterDefinitions()
    {
        EatToken(TokenType.OpenParenthesis);

        List<Parameter> parameters = new();

        while (PeekType != TokenType.CloseParenthesis)
        {
            switch (PeekType)
            {
                case TokenType.StringLiteral: break;

                default: break;
            }

            parameters.Add(new Parameter());

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

    private void ParseStatements()
    {
        while (PeekType != TokenType.CloseScope)
        {
            if (PeekType == TokenType.Identifier)
            {
                switch (Peek.Value)
                {
                    case "if":
                    ParseIfStatement();
                    break;

                    default:
                    ParseUnaryExpression();
                    break;
                }
            }
            else
            {
                throw new ParseException($"Unexpected token {Peek}");
            }
        }
    }

    private void ParseUnaryExpression()
    {
        switch (PeekType)
        {
            case TokenType.Identifier:
            EatToken(TokenType.Identifier);

            if (TryEatToken(TokenType.OpenParenthesis))
            {
                ParseUnaryExpression();
                EatToken(TokenType.CloseParenthesis);
            }
            else if (TryEatToken(TokenType.Assignment))
            {
                throw new NotImplementedException();
            }
            break;

            case TokenType.StringLiteral:
            EatToken(TokenType.StringLiteral);
            break;

            default: throw new ParseException($"Unexpected token {Peek}");
        }
    }

    private void ParseIfStatement()
    {
        EatToken(TokenType.Keyword_If);
        EatToken(TokenType.OpenScope);
        ParseStatements();
        EatToken(TokenType.CloseScope);
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
}
