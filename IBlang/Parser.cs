namespace IBlang;

public class Parser
{
    public bool HasErrors => errors.Count > 0;

    private readonly Token[] tokens;
    private readonly SortedList<int, int> lineEndings;
    private int currentTokenIndex;
    private List<ParseError> errors = new();

    private Token Peek => tokens[currentTokenIndex];
    private TokenType PeekType => Peek.Type;

    public Parser(Token[] tokens, SortedList<int, int> lineEndings)
    {
        this.tokens = tokens;
        this.lineEndings = lineEndings;
    }

    public FileAst Parse()
    {
        Console.WriteLine();

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

                    case TokenType.Comment: EatToken(TokenType.Comment); break;
                    case TokenType.Eof: return new FileAst(functions.ToArray());
                    default: Error(Peek); break;
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
            string type = EatIdentifier();
            string identifier = EatIdentifier();
            parameters.Add(new ParameterDefinition(type, identifier));

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
            _ => Error(Peek),
        };
    }

    private INode ParseFunctionCall()
    {
        string identifier = EatToken(TokenType.Identifier);

        List<INode> args = new();

        if (TryEatToken(TokenType.OpenParenthesis))
        {
            while (!TryEatToken(TokenType.CloseParenthesis))
            {
                args.Add(ParseParameter());

                if (TryEatToken(TokenType.Comma))
                {
                    continue;
                }
                else
                {
                    break;
                }
            }
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
            TokenType.StringLiteral => ParseStringLiteral(),
            TokenType.IntegerLiteral => ParseIntegerLiteral(),
            _ => Error($"{PeekType} is not a valid parameter type"),
        };
    }

    private INode ParseStringLiteral()
    {
        string token = EatToken(TokenType.StringLiteral);
        return new StringLiteral(token);
    }

    private INode ParseIntegerLiteral()
    {
        int token = EatNumber(TokenType.IntegerLiteral);
        return new IntegerLiteral(token);
    }

    /// <summary> if <BinaryExpression> { } </summary>
    private IfStatement ParseIfStatement()
    {
        EatToken(TokenType.Keyword_If);
        ParseBinaryExpression();
        EatToken(TokenType.OpenScope);
        INode[] statements = ParseBlock();
        EatToken(TokenType.CloseScope);

        return new IfStatement(Array.Empty<ParameterDefinition>(), statements);
    }

    /// <summary> <Statement> (=,-,*,/) <Statement> </summary>
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

            default:
            Error(Peek);
            break;
        }

        INode right = ParseStatement();

        return new BinaryExpression(left, right);
    }

    private string EatToken(TokenType type)
    {
        Token p = Peek;

        if (PeekType != type)
        {
            errors.Add(new($"Expected token of type {type} but got {Peek.Type} with value '{Peek.Value}'", Peek.Span));
            return string.Empty;
        }

        currentTokenIndex++;

        return p.Value;
    }

    private int EatNumber(TokenType type)
    {
        Token p = Peek;

        if (PeekType != type)
        {
            errors.Add(new($"Expected {TokenType.IntegerLiteral} but got {Peek.Type} with value '{Peek.Value}'", Peek.Span));
            return 0;
        }

        currentTokenIndex++;

        if (int.TryParse(p.Value, out int result))
        {
            return result;
        }

        throw new ParseException($"Could not parse {p.Value} as an integer");
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
            errors.Add(new($"Expected identifier but got {p.Type} with value '{p.Value}'", p.Span));
            return string.Empty;
        }

        currentTokenIndex++;

        return p.Value;
    }

    private INode Error(string error)
    {
        // TODO move onto next line of tokens

        errors.Add(new(error, Peek.Span));
        currentTokenIndex++;
        return new Garbage(error);
    }

    private INode Error(Token peek)
    {
        string error = $"Unexpected token {peek.Type}: {peek.Value}";
        return Error(error);
    }

    public void ListErrors()
    {
        foreach (ParseError error in errors)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            int line = 0;
            int column = 0;
            int lastIndex = 0;
            foreach ((int index, int newLine) in lineEndings)
            {
                if (error.Span.Start >= lastIndex && error.Span.Start <= index)
                {
                    line = newLine;
                    column = error.Span.Start - lastIndex;
                    break;
                }

                lastIndex = index;
            }

            Console.Error.WriteLine($"{error.Span.File}:{line}:{column} {error.Message}");
            Console.ResetColor();
        }
    }
}
