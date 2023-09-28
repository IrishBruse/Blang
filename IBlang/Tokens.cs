namespace IBlang;

using System;
using System.Diagnostics;

using IBlang.Data;

using OneOf.Types;

[DebuggerStepThrough]
public class Tokens
{
    readonly IEnumerator<Token> tokens;
    readonly SortedList<int, int> lineEndings;
    readonly bool throwOnErrors;

    public Token Peek => tokens.Current;

    List<ParseError> Errors { get; } = new();

    public Tokens(IEnumerator<Token> tokens, SortedList<int, int> lineEndings, bool throwOnErrors = false)
    {
        this.tokens = tokens;
        this.lineEndings = lineEndings;
        this.throwOnErrors = throwOnErrors;
        tokens.MoveNext();
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
            if (throwOnErrors)
            {
                throw new ParseException(error);
            }
            else
            {
                Errors.Add(error);
            }
            return string.Empty;
        }

        tokens.MoveNext();
        return p.Value;
    }

    public void EatKeyword(TokenType type)
    {
        _ = EatToken(type);
    }

    public void Skip()
    {
        tokens.MoveNext();
    }

    public int EatNumber(TokenType type)
    {
        Token p = Peek;

        if (Peek.Type != type)
        {
            ParseError error = new($"Expected {type} but got {Peek.Type} with value '{Peek.Value}'", Peek.Span, new StackTrace(true));
            if (throwOnErrors)
            {
                throw new ParseException(error);
            }
            else
            {
                Errors.Add(error);
            }
            return 0;
        }

        tokens.MoveNext();

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
            tokens.MoveNext();
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
            if (throwOnErrors)
            {
                throw new ParseException(error);
            }
            else
            {
                Errors.Add(error);
            }
            return string.Empty;
        }

        tokens.MoveNext();

        return p.Value;
    }

    public Error<string> Error(string message)
    {
        ParseError error = new(message, Peek.Span, new StackTrace(true));
        if (throwOnErrors)
        {
            throw new ParseException(error);
        }
        else
        {
            Errors.Add(error);
        }

        tokens.MoveNext();

        return new Error<string>(message);
    }

    public bool Eof()
    {
        return Peek.Type == TokenType.Eof;
    }

    public void AddError(ParseError error)
    {
        if (throwOnErrors)
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
