namespace IBlang;

using System.Diagnostics;

using IBlang.Data;

[DebuggerStepThrough]
public class Project
{
    public Token Peek => tokens.Current;
    public List<ParseError> Errors { get; } = [];
    public readonly SortedList<int, int> LineEndings;
    public readonly Dictionary<string, int> Strings = [];

    readonly IEnumerator<Token> tokens;

    static readonly bool ThrowOnErrors;

    public Project(IEnumerator<Token> tokens, SortedList<int, int> lineEndings)
    {
        this.tokens = tokens;
        LineEndings = lineEndings;
        _ = tokens.MoveNext();
    }

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

    public void AddString(string value)
    {
        Strings.Add(value, Strings.Count);
    }

    public int GetString(string value)
    {
        if (Strings.TryGetValue(value, out int index))
        {
            return index;
        }

        throw new Exception("String not found");
    }
}
