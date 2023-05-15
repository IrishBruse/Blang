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
        try
        {
            List<FunctionDecleration> functions = new();
            while (true)
            {
                switch (PeekType)
                {
                    case TokenType.Keyword_Func:
                    functions.Add(ParseFunctionDecleration());
                    break;

                    case TokenType.Eof:
                    return new FileAst(functions.ToArray());

                    default: throw new ParseException($"Unexpected token {Peek}");
                }
            }
        }
        catch (ParseException e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(e.Message);
            Console.Error.WriteLine(e.StackTrace);
            Console.ResetColor();
        }
        return new FileAst(Array.Empty<FunctionDecleration>());
    }

    private FunctionDecleration ParseFunctionDecleration()
    {
        EatToken(TokenType.Keyword_Func);

        string name = EatToken(TokenType.Identifier);
        ParameterDefinition[] parameters = ParseParameterDefinitions();
        INode[] statements = ParseBlock();

        return new FunctionDecleration(name, parameters, statements);
    }

    private ParameterDefinition[] ParseParameterDefinitions()
    {
        EatToken(TokenType.OpenParenthesis);

        List<ParameterDefinition> parameters = new();

        while (PeekType != TokenType.CloseParenthesis)
        {
            parameters.Add(new ParameterDefinition(EatIdentifier(), EatIdentifier()));

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

    private INode[] ParseBlock()
    {
        EatToken(TokenType.OpenScope);

        List<INode> statements = new();

        while (PeekType != TokenType.CloseScope)
        {
            statements.Add(ParseStatement());
        }

        EatToken(TokenType.CloseScope);

        return statements.ToArray();
    }

    private INode ParseStatement()
    {
        return PeekType switch
        {
            TokenType.Identifier => ParseFunctionCall(),
            TokenType.Keyword_If => ParseIfStatement(),
            _ => throw new ParseException($"Unexpected token {Peek}"),
        };
    }

    private INode ParseFunctionCall()
    {
        string identifier = EatToken(TokenType.Identifier);

        List<INode> args = new();

        if (TryEatToken(TokenType.OpenParenthesis))
        {
            args.Add(ParseParameter());
            EatToken(TokenType.CloseParenthesis);
        }
        else if (TryEatToken(TokenType.Assignment))
        {
            throw new NotImplementedException();
        }
        return new FunctionCall(identifier, args.ToArray());
    }

    private INode ParseParameter()
    {
        return PeekType switch
        {
            TokenType.Identifier => ParseFunctionCall(),
            TokenType.StringLiteral => new StringLiteral(EatToken(TokenType.StringLiteral)),
            TokenType.IntegerLiteral => new IntegerLiteral(EatToken(TokenType.IntegerLiteral)),
            _ => throw new ParseException($"Unexpected token {Peek}"),
        };
    }

    private IfStatement ParseIfStatement()
    {
        EatToken(TokenType.Keyword_If);
        ParseBinaryExpression();
        EatToken(TokenType.OpenScope);
        INode[] statements = ParseBlock();
        EatToken(TokenType.CloseScope);

        return new IfStatement(Array.Empty<ParameterDefinition>(), statements);
    }

    private BinaryExpression ParseBinaryExpression()
    {
        INode left = ParseStatement();

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

        INode right = ParseStatement();

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
            throw new ParseException($"Expected identifier but got {p.Type} with value '{p.Value}'");
        }

        currentTokenIndex++;

        return p.Value;
    }
}
