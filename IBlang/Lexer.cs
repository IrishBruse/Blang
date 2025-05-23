namespace IBlang;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class Lexer(CompilationFlags Flags)
{
    private readonly List<int> lineEndingIndexes = [0];

    public StreamReader Source = null!;
    public string FilePath = "";

    const ConsoleColor CommentColor = ConsoleColor.DarkGray;
    const ConsoleColor WhitespaceColor = ConsoleColor.DarkGray;
    const ConsoleColor BracketsColor = ConsoleColor.DarkGreen;
    const ConsoleColor OperatorColor = ConsoleColor.Red;
    const ConsoleColor NumberColor = ConsoleColor.Cyan;
    const ConsoleColor StringColor = ConsoleColor.Yellow;
    const ConsoleColor IdentifierColor = ConsoleColor.Gray;

    int endIndex;
    int startIndex;

    public IEnumerator<Token> Lex(string text)
    {
        MemoryStream stream = new(Encoding.UTF8.GetBytes(text));
        StreamReader reader = new(stream);
        return Lex(reader, "");
    }

    public IEnumerator<Token> Lex(StreamReader fileStream, string path)
    {
        Source = fileStream;
        FilePath = path;
        while (!Source.EndOfStream)
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

                    ';' => LexOperator(TokenType.Semicolon),

                    _ => new Token(TokenType.Garbage, c.ToString(), EndTokenRange())
                };
            }
        }

        Source.Close();

        yield return new Token(TokenType.Eof, string.Empty, EndTokenRange());
    }

    void EatWhitespace(char c)
    {
        if (c == '\r')
        {
            _ = Next(display: Flags.HasFlag(CompilationFlags.Whitespace) ? "\\r" : "", foreground: WhitespaceColor);
        }
        else if (c == '\n')
        {
            lineEndingIndexes.Add(startIndex);
            _ = Next(display: Flags.HasFlag(CompilationFlags.Whitespace) ? "\\n\n" : "\n", foreground: WhitespaceColor);
        }
        else if (c == '\t')
        {
            _ = Next(display: Flags.HasFlag(CompilationFlags.Whitespace) ? "»   " : "    ", foreground: WhitespaceColor);
        }
        else
        {
            // Eat all other whitespace
            _ = Next(display: Flags.HasFlag(CompilationFlags.Whitespace) ? "·" : " ", foreground: WhitespaceColor);
        }

        EndTokenRange();
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
        else if (c == '/' && p == '*') // Multiline line comment
        {
            return LexMultiLineComment();
        }

        return new Token(type, op, EndTokenRange());
    }

    Token LexSingleLineComment()
    {
        if (Flags.HasFlag(CompilationFlags.Print))
        {
            Console.CursorLeft--;
            Print('/', foreground: CommentColor);
        }

        StringBuilder comment = new("/");

        while (!IsLineBreak(Peek()) && !Source.EndOfStream)
        {
            _ = comment.Append(Next(CommentColor));
        }

        return new Token(TokenType.Comment, comment.ToString(), EndTokenRange());
    }

    Token LexMultiLineComment()
    {
        if (Flags.HasFlag(CompilationFlags.Print))
        {
            Console.CursorLeft--;
            Print('/', foreground: CommentColor);
        }

        StringBuilder comment = new("/*");

        Next(); // Eat *

        while (!Source.EndOfStream)
        {
            char c = Next(CommentColor);
            if (c == '\n')
            {
                _ = comment.Append("\\n");
            }
            else
            {
                _ = comment.Append(c);
            }

            if (c == '*' && Peek() == '/')
            {
                comment.Append(Next());
                break;
            }
        }

        return new Token(TokenType.Comment, comment.ToString(), EndTokenRange());
    }

    Token LexBracket(TokenType type)
    {
        char c = Next(BracketsColor);

        return new Token(type, c.ToString(), EndTokenRange());
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

        return new Token(TokenType.StringLiteral, literal.ToString(), EndTokenRange());
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

        string identifier = identifierBuilder.ToString();

        return identifier switch
        {
            "extrn" => new Token(TokenType.ExternKeyword, identifier, EndTokenRange()),
            "if" => new Token(TokenType.IfKeyword, identifier, EndTokenRange()),
            "while" => new Token(TokenType.WhileKeyword, identifier, EndTokenRange()),
            "auto" => new Token(TokenType.AutoKeyword, identifier, EndTokenRange()),
            _ => new Token(TokenType.Identifier, identifier, EndTokenRange()),
        };
    }

    char Peek()
    {
        return (char)Source.Peek();
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

        return new Token(TokenType.IntegerLiteral, number.ToString(), EndTokenRange());
    }

    static bool IsLineBreak(char c) => c is '\n' or '\r';

    char Next(ConsoleColor foreground = ConsoleColor.White, string? display = null)
    {
        char c = (char)Source.Read();

        endIndex++;

        if (Flags.HasFlag(CompilationFlags.Print))
        {
            if (display != null)
            {
                Print(display, foreground);
            }
            else
            {
                Print(c, foreground);
            }
        }

        return c;
    }

    static void Print(char c, ConsoleColor foreground = ConsoleColor.White)
    {
        // Console.BackgroundColor = background;
        Console.ForegroundColor = foreground;
        Console.Write(c);
        Console.ResetColor();
    }

    static void Print(string str, ConsoleColor foreground = ConsoleColor.White)
    {
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
        int start = startIndex;
        int end = endIndex;
        (int line, int column) = GetLineColumnFromIndex(start);
        return new Range(FilePath, start, end, line, column);
    }

    public (int Line, int Column) GetLineColumnFromIndex(int index)
    {
        if (lineEndingIndexes.Count == 0)
        {
            return (1, index + 1);
        }

        int line = 0;
        int lineOffset = 0;
        for (int i = 0; i < lineEndingIndexes.Count; i++)
        {
            if (index > lineEndingIndexes[i])
            {
                lineOffset = lineEndingIndexes[i];
                line = i;
            }
        }

        int column = index - lineOffset;

        return (line + 1, column);
    }
}

public enum CompilationFlags
{
    None,
    Whitespace,
    Print
}
