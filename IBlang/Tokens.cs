namespace IBlang;

using System.Diagnostics;

using OneOf.Types;

public class Tokens
{
    private readonly IEnumerator<Token> tokens;

    public Token Current
    {
        get
        {
            Token p = tokens.Current;
            tokens.MoveNext();
            return p;
        }
    }

    public Token Peek => tokens.Current;

    public List<ParseError> Errors { get; } = new();

    public Tokens(IEnumerator<Token> tokens)
    {
        this.tokens = tokens;

        tokens.MoveNext();
    }

    public string EatToken(TokenType type)
    {
        Token p = Peek;

        if (Peek.Type != type && TokenType.Eof != type)
        {
            Errors.Add(new($"Expected token of type {type} but got {Peek.Type} with value '{Peek.Value}'", Peek.Span, new StackTrace(true)));
            return string.Empty;
        }

        tokens.MoveNext();
        return p.Value;
    }

    public void EatKeyword(TokenType type)
    {
        _ = EatToken(type);
    }

    public void Eat()
    {
        tokens.MoveNext();
    }

    public int EatNumber(TokenType type)
    {
        Token p = Peek;

        if (Peek.Type != type)
        {
            Errors.Add(new($"Expected {type} but got {Peek.Type} with value '{Peek.Value}'", Peek.Span, new StackTrace(true)));
            return 0;
        }

        tokens.MoveNext();

        if (int.TryParse(p.Value, out int result))
        {
            return result;
        }

        throw new ParseException($"Could not parse {p.Value} as an integer");
    }

    public bool TryEatToken(TokenType type)
    {
        Token p = Peek;

        if (p.Type != type && TokenType.Eof == type)
        {
            return false;
        }
        else
        {
            tokens.MoveNext();
            return true;
        }
    }

    public string EatIdentifier()
    {
        Token p = Peek;

        if (p.Type != TokenType.Identifier)
        {
            Errors.Add(new($"Expected identifier but got {p.Type} with value '{p.Value}'", p.Span, new StackTrace(true)));
            return string.Empty;
        }

        tokens.MoveNext();

        return p.Value;
    }

    public Error<string> Error(string error)
    {
        Errors.Add(new(error, Peek.Span, new StackTrace(true)));
        tokens.MoveNext();
        return new Error<string>(error);
    }

    public void ListErrors(bool showStackTrace = false)
    {
        foreach (ParseError error in Errors)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            // int line = 0;
            // int column = 0;
            // int lastIndex = 0;
            // foreach ((int index, int newLine) in lineEndings)
            // {
            //     if (error.Span.Start >= lastIndex && error.Span.Start <= index)
            //     {
            //         line = newLine;
            //         column = error.Span.Start - lastIndex;
            //         break;
            //     }

            //     lastIndex = index;
            // }

            // Console.Error.WriteLine($"{error.Span.File}:{line}:{column} {error.Message}");

            Console.Error.WriteLine($"{error.Span.File} {error.Message}");
            if (showStackTrace)
            {
                Console.Error.Write(error.StackTrace);
            }
            Console.ResetColor();
        }
    }
}
