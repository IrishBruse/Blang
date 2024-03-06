namespace IBlang;

using System;
using System.Diagnostics;

using IBlang.Data;

[DebuggerStepThrough]
public class Tokens
{
    static readonly bool ThrowOnErrors = false;

    readonly IEnumerator<Token> tokens;
    readonly SortedList<int, int> lineEndings;

    public Token Peek => tokens.Current;

    List<ParseError> Errors { get; } = [];

    public Tokens(IEnumerator<Token> tokens, SortedList<int, int> lineEndings)
    {
        this.tokens = tokens;
        this.lineEndings = lineEndings;
        _ = tokens.MoveNext();
    }

    [DebuggerStepThrough]
    public string EatToken(TokenType type, string? errorOverride = null)
    {
        Token p = Peek;

        if (Peek.Type != type && TokenType.Eof != type)
        {
            string message;
            if (errorOverride != null)
            {
                message = errorOverride;
            }
            else
            {
                message = $"Expected token of type {type} but got {Peek.Type} with value '{Peek.Value}'";
            }

            ParseError error = new(message, Peek.Span, new StackTrace(true));
            if (ThrowOnErrors)
            {
                throw new ParseException(error);
            }
            else
            {
                Errors.Add(error);
            }
            return string.Empty;
        }

        _ = tokens.MoveNext();
        return p.Value;
    }

    public void EatKeyword(TokenType type)
    {
        _ = EatToken(type);
    }

    public void Skip()
    {
        _ = tokens.MoveNext();
    }

    public int EatNumber(TokenType type)
    {
        Token p = Peek;

        if (Peek.Type != type)
        {
            ParseError error = new($"Expected {type} but got {Peek.Type} with value '{Peek.Value}'", Peek.Span, new StackTrace(true));
            if (ThrowOnErrors)
            {
                throw new ParseException(error);
            }
            else
            {
                Errors.Add(error);
            }
            return 0;
        }

        _ = tokens.MoveNext();

        if (int.TryParse(p.Value, out int result))
        {
            return result;
        }

        return int.MinValue;
    }

    public bool TryEatToken(TokenType type)
    {
        Token p = Peek;

        if (p.Type == type)
        {
            _ = tokens.MoveNext();
            return true;
        }
        else
        {
            return false;
        }
    }

    public string EatIdentifier()
    {
        Token p = Peek;

        if (p.Type != TokenType.Identifier)
        {
            ParseError error = new($"Expected identifier but got {p.Type} with value '{p.Value}'", p.Span, new StackTrace(true));
            if (ThrowOnErrors)
            {
                throw new ParseException(error);
            }
            else
            {
                Errors.Add(error);
            }
            return string.Empty;
        }

        _ = tokens.MoveNext();

        return p.Value;
    }

    public Error Error(string message)
    {
        ParseError error = new(message, Peek.Span, new StackTrace(true));
        if (ThrowOnErrors)
        {
            throw new ParseException(error);
        }
        else
        {
            Errors.Add(error);
        }

        _ = tokens.MoveNext();

        return new Error(message);
    }

    public bool Eof()
    {
        return Peek.Type == TokenType.Eof;
    }

    public void AddError(ParseError error)
    {
        if (ThrowOnErrors)
        {
            throw new ParseException(error);
        }
        else
        {
            Errors.Add(error);
        }
    }

    public void ListErrors(bool showStackTrace = false)
    {
        if (Errors.Count > 0)
        {
            Console.WriteLine("\n\n-------- Errors --------");
        }

        foreach (ParseError error in Errors)
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

            if (showStackTrace)
            {
                Console.Error.Write(error.StackTrace);
            }
            Console.ResetColor();
        }
    }
}
