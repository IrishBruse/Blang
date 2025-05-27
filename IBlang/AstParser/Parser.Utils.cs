namespace IBlang.AstParser;

using System;
using System.Collections.Generic;
using IBlang.Exceptions;
using IBlang.Tokenizer;

public partial class Parser
{
    public readonly List<string> Errors = [];

    int end = 0;
    TokenType Peek()
    {
        return tokens.Current.TokenType;
    }

    Token Next()
    {
        Token token = tokens.Current;
        end = token.Range.End;
        tokens.MoveNext();
        return token;
    }

    Token Eat(TokenType type)
    {
        int lastEnd = end;

        Token token = Next();

        if (token.TokenType != type)
        {
            throw new ParserException(data.GetFileLocation(lastEnd) + $": Expected {type} but got {token.TokenType}");
        }

        return token;
    }

    void Error(ParserException e)
    {
        if (Flags.GetValueOrDefault("trace", "false") == "true")
        {
            Errors.Add(e.Error + "\n" + e.StackTrace);
        }
        else
        {
            Errors.Add(e.ToString());
        }
    }

    void Error(string error)
    {
        string msg = data.GetFileLocation(end) + ": " + error;

        if (Flags.GetValueOrDefault("trace", "false") == "true")
        {
            string trace = string.Join("\n", Environment.StackTrace.ToString().Split('\n')[2..]);
            Errors.Add(msg + "\n" + trace);
        }
        else
        {
            Errors.Add(msg);
        }
    }
}
