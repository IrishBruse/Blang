namespace BLang.AstParser;

using System;
using System.Collections.Generic;
using BLang.Exceptions;
using BLang.Tokenizer;
using BLang.Utility;

public partial class Parser
{
    public readonly List<string> Errors = [];

    SourceRange previousTokenRange;

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
        previousTokenRange = token.Range;
        tokens.MoveNext();

        if (Flags.Instance.Tokens)
        {
            Console.WriteLine(tokens.Current);
        }

        return token;
    }

    Token Eat(TokenType type)
    {
        Token token = Next();

        if (token.TokenType != type)
        {
            int lastEnd = previousTokenRange.End;
            throw new ParserException(data.GetFileLocation(lastEnd) + $" Expected {type} but got {token.TokenType}");
        }

        return token;
    }

    void Error(string error)
    {
        string msg = data.GetFileLocation(previousTokenRange.End) + ": " + error;

        if (Flags.Instance.Debug)
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

    Variable ParseVariable()
    {
        Token variable = Eat(TokenType.Identifier);
        return new Variable(variable.Content)
        {
            Range = variable.Range
        };
    }

    IntValue ParseInteger()
    {
        Token integer = Eat(TokenType.IntegerLiteral);
        return new IntValue(int.Parse(integer.Content))
        {
            Range = integer.Range
        };
    }

    StringValue ParseString()
    {
        Token str = Eat(TokenType.StringLiteral);
        return new StringValue(str.Content)
        {
            Range = str.Range
        };
    }
}
