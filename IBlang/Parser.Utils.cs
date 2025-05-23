namespace IBlang;

using System;
using IBlang.Exceptions;

public partial class Parser
{
    Token Peek()
    {
        return tokens.Current;
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
