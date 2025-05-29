namespace IBlang.AstParser;

using System;
using System.Collections.Generic;
using IBlang.Exceptions;
using IBlang.Tokenizer;
using IBlang.Utility;

public partial class Parser
{
    public readonly List<string> Errors = [];

    int end = 0;

    public bool Peek(TokenType type)
    {
        return tokens.Current.TokenType == type;
    }

    public TokenType Peek()
    {
        return tokens.Current.TokenType;
    }

    Token Next()
    {
        Token token = tokens.Current;
        end = token.Range.End;
        tokens.MoveNext();

        if (Flags.Tokens)
        {
            Console.WriteLine(tokens.Current);
        }

        return token;
    }

    Token Eat(TokenType type)
    {
        int lastEnd = end;

        Token token = Next();

        if (token.TokenType != type)
        {
            throw new ParserException(data.GetFileLocation(lastEnd) + $": Expected {TokenTypeToString(type)} but got {TokenTypeToString(token.TokenType)}");
        }

        return token;
    }

    static string TokenTypeToString(TokenType type) => type switch
    {
        TokenType.Semicolon => ";",

        TokenType.OpenScope => "{",
        TokenType.CloseScope => "}",

        TokenType.OpenParenthesis => "(",
        TokenType.CloseParenthesis => ")",

        TokenType.OpenBracket => "[",
        TokenType.CloseBracket => "]",
        _ => type.ToString(),
    };

    void Error(string error)
    {
        string msg = data.GetFileLocation(end) + ": " + error;

        if (Flags.Debug)
        {
            string trace = string.Join("\n", Environment.StackTrace.ToString().Split('\n')[2..]);
            Errors.Add(msg + "\n" + trace);
        }
        else
        {
            Errors.Add(msg);
        }
    }

    void EatComments()
    {
        while (Peek(TokenType.Comment))
        {
            Eat(TokenType.Comment);
        }
    }

}
