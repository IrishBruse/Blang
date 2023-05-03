namespace IBlang;

public class Parser
{
    private readonly Token[] tokens;
    private int currentTokenIndex;

    public Parser(Token[] tokens)
    {
        this.tokens = tokens;
    }

    public Ast Parse()
    {
        List<FunctionDecleration> functions = new();
        while (true)
        {
            switch (Peek)
            {
                case Token t when t.Type == TokenType.Identifier && t.Value == "func": functions.Add(ParseFunctionDecleration()); break;
                case Token t when t.Type == TokenType.Eof: return new Ast(functions.ToArray());
                default: throw new ParseException($"Unexpected token {Peek}");
            }
        }
    }

    private FunctionDecleration ParseFunctionDecleration()
    {
        List<Parameter> parameters = new();
        List<Statement> statements = new();

        if (TryEatToken(TokenType.Identifier))
        {
            EatToken(TokenType.Identifier);
            EatToken(TokenType.OpenParenthesis);

            while (TryEatToken(TokenType.Identifier)) // Type
            {
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

            EatToken(TokenType.OpenScope);

            EatToken(TokenType.CloseScope);
        }

        return new FunctionDecleration(parameters.ToArray(), statements.ToArray());
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

    private bool TryEatKeyword(string keyword)
    {
        Token p = Peek;

        if (p.Type != TokenType.Identifier || p.Value != keyword)
        {
            return false;
        }
        else
        {
            currentTokenIndex++;
            return true;
        }
    }

    private Token Peek => tokens[currentTokenIndex];
    private TokenType PeekType => Peek.Type;
}
