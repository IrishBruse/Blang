namespace IBlang.AstParser;

using IBlang.Exceptions;
using IBlang.Tokenizer;

public partial class Parser
{
    TokenType Peek()
    {
        return tokens.Current.TokenType;
    }

    Token Next()
    {
        Token token = tokens.Current;
        // Console.WriteLine(token);
        tokens.MoveNext();
        return token;
    }

    Token Eat(TokenType type)
    {
        Token token = Next();

        if (token.TokenType != type)
        {
            throw new InvalidTokenException($"{token.TokenType} != {type}");
        }

        return token;
    }
}
