namespace IBlang;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

public class Lexer
{
    public SortedList<int, int> LineEndings { get; private set; } = [];

    // public static readonly Dictionary<string, TokenType> Keywords = new()
    // {
    //     { "func", TokenType.Keyword_Func },
    //     { "true", TokenType.Keyword_True },
    //     { "false", TokenType.Keyword_False },
    // };

    // public static readonly HashSet<TokenType> BinaryOperators = new()
    // {
    //     { TokenType.Addition },
    //     { TokenType.Subtraction },
    //     { TokenType.Multiplication },
    //     { TokenType.Division },
    //     { TokenType.Modulo },
    //     { TokenType.BitwiseAnd },
    //     { TokenType.BitwiseOr },
    //     { TokenType.BitwiseShiftLeft },
    //     { TokenType.BitwiseShiftRight },
    // };

    // static readonly Dictionary<string, TokenType> ControlflowKeywords = new()
    // {
    //     { "if", TokenType.Keyword_If },
    //     { "else", TokenType.Keyword_Else },
    //     { "return", TokenType.Keyword_Return },
    // };

    const ConsoleColor CommentColor = ConsoleColor.DarkGray;
    const ConsoleColor WhitespaceColor = ConsoleColor.DarkGray;
    const ConsoleColor KeywordColor = ConsoleColor.Blue;
    const ConsoleColor BracketsColor = ConsoleColor.DarkGreen;
    const ConsoleColor OperatorColor = ConsoleColor.Red;
    const ConsoleColor NumberColor = ConsoleColor.Cyan;
    const ConsoleColor StringColor = ConsoleColor.Yellow;
    const ConsoleColor IdentifierColor = ConsoleColor.Gray;
    const ConsoleColor ControlflowColor = ConsoleColor.Magenta;

    StreamReader sourceFile;
    readonly string file;

    readonly CompilationFlags flags;

    int endIndex;
    int startIndex;
    int line;

    public Lexer(StreamReader sourceFile, string file, CompilationFlags flags = CompilationFlags.None)
    {
        this.sourceFile = sourceFile;
        this.file = file;

        this.flags = flags;
    }

    // public Lexer(string sourceText, CompilationFlags flags = CompilationFlags.None)
    // {
    //     file = "__NOFILE__.ib";
    //     MemoryStream stream = new();
    //     StreamWriter writer = new(stream);
    //     writer.Write(sourceText);
    //     writer.Flush();
    //     stream.Position = 0;

    //     sourceFile = new StreamReader(stream);

    //     this.flags = flags;
    // }

    // static Lexer()
    // {
    //     foreach ((string key, TokenType value) in ControlflowKeywords)
    //     {
    //         Keywords.Add(key, value);
    //     }
    // }

    public IEnumerator<Token> Lex()
    {
        // if (flags.HasFlag(CompilationFlags.Print))
        // {
        //     Console.WriteLine("-------- Lexer  --------");
        // }

        while (!sourceFile.EndOfStream)
        {
            char c = Peek();

            StartTokenRange();

            if (char.IsWhiteSpace(c))
            {
                EatWhitespace(c);
            }
            else if (char.IsLetter(c))
            {
                yield return LexIdentifier();
            }
            else if (char.IsNumber(c))
            {
                yield return LexNumber();
            }
            else
            {
                yield return c switch
                {
                    '"' => LexString(),

                    '[' => LexBracket(TokenType.OpenBracket),
                    ']' => LexBracket(TokenType.CloseBracket),

                    '(' => LexBracket(TokenType.OpenParenthesis),
                    ')' => LexBracket(TokenType.CloseParenthesis),

                    '{' => LexBracket(TokenType.OpenScope),
                    '}' => LexBracket(TokenType.CloseScope),

                    '<' => LexOperator(TokenType.LessThan),
                    '>' => LexOperator(TokenType.GreaterThan),
                    '+' => LexOperator(TokenType.Addition),
                    '-' => LexOperator(TokenType.Subtraction),
                    '*' => LexOperator(TokenType.Multiplication),
                    '/' => LexOperator(TokenType.Division),
                    '&' => LexOperator(TokenType.BitwiseAnd),
                    '|' => LexOperator(TokenType.BitwiseOr),
                    '%' => LexOperator(TokenType.Modulo),
                    '!' => LexOperator(TokenType.LogicalNot),
                    '=' => LexOperator(TokenType.Assignment),

                    _ => new Token(c.ToString(), TokenType.Garbage, EndTokenRange())
                };
            }
        }

        sourceFile.Close();

        yield return new Token(string.Empty, TokenType.Eof, EndTokenRange());
    }

    void EatWhitespace(char c)
    {
        if (c == '\r')
        {
            _ = Next(display: flags.HasFlag(CompilationFlags.Whitespace) ? "\\r" : "", foreground: WhitespaceColor);
        }
        else if (c == '\n')
        {
            line++;
            LineEndings.Add(endIndex, line);
            _ = Next(display: flags.HasFlag(CompilationFlags.Whitespace) ? "\\n\n" : "\n", foreground: WhitespaceColor);
        }
        else if (c == '\t')
        {
            _ = Next(display: flags.HasFlag(CompilationFlags.Whitespace) ? "»   " : "    ", foreground: WhitespaceColor);
        }
        else
        {
            // Eat all other whitespace
            _ = Next(display: flags.HasFlag(CompilationFlags.Whitespace) ? "·" : " ", foreground: WhitespaceColor);
        }
    }

    Token LexOperator(TokenType type)
    {
        char c = Next(OperatorColor);
        char p = Peek();

        string op = c.ToString();

        if (c == '<' && p == '=')
        {
            type = TokenType.LessThanEqual;
            op += Next(OperatorColor);
        }
        else if (c == '>' && p == '=')
        {
            type = TokenType.GreaterThanEqual;
            op += Next(OperatorColor);
        }
        else if (c == '=' && p == '=')
        {
            type = TokenType.EqualEqual;
            op += Next(OperatorColor);
        }
        else if (c == '!' && p == '=')
        {
            type = TokenType.NotEqual;
            op += Next(OperatorColor);
        }
        else if (c == '&' && p == '&')
        {
            type = TokenType.LogicalAnd;
            op += Next(OperatorColor);
        }
        else if (c == '|' && p == '|')
        {
            type = TokenType.LogicalOr;
            op += Next(OperatorColor);
        }
        else if (c == '+' && p == '=')
        {
            type = TokenType.AdditionAssignment;
            op += Next(OperatorColor);
        }
        else if (c == '-' && p == '=')
        {
            type = TokenType.SubtractionAssignment;
            op += Next(OperatorColor);
        }
        else if (c == '*' && p == '=')
        {
            type = TokenType.MultiplicationAssignment;
            op += Next(OperatorColor);
        }
        else if (c == '/' && p == '=')
        {
            type = TokenType.DivisionAssignment;
            op += Next(OperatorColor);
        }
        else if (c == '%' && p == '=')
        {
            type = TokenType.ModuloAssignment;
            op += Next(OperatorColor);
        }
        else if (c == '<' && p == '<')
        {
            type = TokenType.BitwiseShiftLeft;
            op += Next(OperatorColor);
        }
        else if (c == '>' && p == '>')
        {
            type = TokenType.BitwiseShiftRight;
            op += Next(OperatorColor);
        }
        else if (c == '/' && p == '/') // Single line comment
        {
            return LexSingleLineComment();
        }

        return new Token(op, type, EndTokenRange());
    }

    Token LexSingleLineComment()
    {
        if (flags.HasFlag(CompilationFlags.Print))
        {
            Console.CursorLeft--;
            Print('/', foreground: CommentColor);
        }

        StringBuilder comment = new("/");

        while (!IsLineBreak(Peek()) && !sourceFile.EndOfStream)
        {
            _ = comment.Append(Next(CommentColor));
        }

        return new Token(comment.ToString(), TokenType.Comment, EndTokenRange());
    }

    Token LexBracket(TokenType type)
    {
        char c = Next(BracketsColor);

        return new Token(c.ToString(), type, EndTokenRange());
    }

    Token LexString()
    {
        StringBuilder literal = new();

        // Eat first "
        _ = Next(StringColor);

        char c;

        do
        {
            c = Next(StringColor);
            _ = literal.Append(c);
        }
        while (Peek() != '"' && !IsLineBreak(Peek()));

        _ = Next(StringColor);

        return new Token(literal.ToString(), TokenType.StringLiteral, EndTokenRange());
    }

    Token LexIdentifier()
    {
        StringBuilder identifierBuilder = new();
        char c;

        do
        {
            c = Next(IdentifierColor);
            _ = identifierBuilder.Append(c);
        }
        while (char.IsLetterOrDigit(Peek()) && !IsLineBreak(c));

        // string identifier = identifierBuilder.ToString().ToLower(CultureInfo.CurrentCulture);

        // if (Keywords.TryGetValue(identifier, out TokenType keyword))
        // {
        //     if (flags.HasFlag(CompilationFlags.Print))
        //     {
        //         // Recolor keywords
        //         if (!Console.IsOutputRedirected)
        //         {
        //             Console.CursorLeft -= identifier.Length;
        //         }

        //         ConsoleColor color = ControlflowKeywords.TryGetValue(identifier, out _) ? ControlflowColor : KeywordColor;
        //         Print(identifier, foreground: color);
        //     }

        //     return new Token(identifierBuilder.ToString(), keyword, EndTokenRange());
        // }
        // else
        // {
        return new Token(identifierBuilder.ToString(), TokenType.Identifier, EndTokenRange());
        // }
    }

    char Peek()
    {
        return (char)sourceFile.Peek();
    }

    Token LexNumber()
    {
        StringBuilder number = new();

        char c;

        do
        {
            c = Next(NumberColor);
            _ = number.Append(c);
        }
        while (char.IsDigit(Peek()));

        return new Token(number.ToString(), TokenType.IntegerLiteral, EndTokenRange());
    }

    static bool IsLineBreak(char c)
    {
        return c is '\n' or '\r';
    }

    char Next(ConsoleColor foreground = ConsoleColor.White, ConsoleColor background = ConsoleColor.Black, string? display = null)
    {
        char c = (char)sourceFile.Read();

        endIndex++;

        if (flags.HasFlag(CompilationFlags.Print))
        {
            if (display != null)
            {
                Print(display, background, foreground);
            }
            else
            {
                Print(c, background, foreground);
            }
        }

        return c;
    }

    static void Print(char c, ConsoleColor background = ConsoleColor.Black, ConsoleColor foreground = ConsoleColor.White)
    {
        Console.BackgroundColor = background;
        Console.ForegroundColor = foreground;
        Console.Write(c);
        Console.ResetColor();
    }

    static void Print(string str, ConsoleColor background = ConsoleColor.Black, ConsoleColor foreground = ConsoleColor.White)
    {
        Console.BackgroundColor = background;
        Console.ForegroundColor = foreground;
        Console.Write(str);
        Console.ResetColor();
    }

    public void StartTokenRange()
    {
        startIndex = endIndex;
    }

    public Range EndTokenRange()
    {
        return new Range(startIndex, endIndex);
    }
}

public enum CompilationFlags
{
    None,
    Whitespace,
    Print
}
