namespace IBlang;

using System;
using System.Globalization;
using System.IO;
using System.Text;

public class Lexer : IDisposable
{
    public SortedList<int, int> LineEndings { get; private set; } = new();

    public static readonly Dictionary<string, TokenType> Keywords = new()
    {
        { "func", TokenType.Keyword_Func },

        { "true", TokenType.Keyword_True },
        { "false", TokenType.Keyword_False },
    };

    private static readonly Dictionary<string, TokenType> ControlflowKeywords = new()
    {
        { "if", TokenType.Keyword_If },
        { "else", TokenType.Keyword_Else },
        { "return", TokenType.Keyword_Return },
    };

    private const ConsoleColor CommentColor = ConsoleColor.DarkGray;
    private const ConsoleColor WhitespaceColor = ConsoleColor.DarkGray;
    private const ConsoleColor KeywordColor = ConsoleColor.Blue;
    private const ConsoleColor BracketsColor = ConsoleColor.DarkGreen;
    private const ConsoleColor ErrorColor = ConsoleColor.DarkRed;
    private const ConsoleColor OperatorColor = ConsoleColor.Red;
    private const ConsoleColor NumberColor = ConsoleColor.Cyan;
    private const ConsoleColor StringColor = ConsoleColor.Yellow;
    private const ConsoleColor IdentifierColor = ConsoleColor.Gray;
    private const ConsoleColor ControlflowColor = ConsoleColor.Magenta;

    private StreamReader sourceFile;
    private readonly string file;
    private List<Token> tokens = new();
    private readonly bool debug;
    private readonly bool whitespace;
    private int endIndex;
    private int startIndex;
    private int line;

    public Lexer(StreamReader sourceFile, string file, bool debug = false, bool whitespace = false)
    {
        this.sourceFile = sourceFile;
        this.file = file;
        this.debug = debug;
        this.whitespace = whitespace;
    }

    public Lexer(string sourceText, bool debug = false, bool whitespace = false)
    {
        MemoryStream stream = new();
        StreamWriter writer = new(stream);
        writer.Write(sourceText);
        writer.Flush();
        stream.Position = 0;

        sourceFile = new StreamReader(stream);

        this.debug = debug;
        this.whitespace = whitespace;
    }

    static Lexer()
    {
        foreach ((string key, TokenType value) in ControlflowKeywords)
        {
            Keywords.Add(key, value);
        }
    }

    public Token[] Lex()
    {
        if (debug)
        {
            Console.WriteLine("-------- Lexer --------");
        }

        while (!sourceFile.EndOfStream)
        {
            char c = Peek();

            startIndex = endIndex;

            if (char.IsWhiteSpace(c))
            {
                LexWhitespace(c);
            }
            else if (char.IsLetter(c))
            {
                LexIdentifier();
            }
            else if (char.IsNumber(c))
            {
                LexNumber();
            }
            else
            {
                switch (c)
                {
                    case '"': LexString(); break;

                    case '[': LexBracket(TokenType.OpenBracket); break;
                    case ']': LexBracket(TokenType.CloseBracket); break;

                    case '(': LexBracket(TokenType.OpenParenthesis); break;
                    case ')': LexBracket(TokenType.CloseParenthesis); break;

                    case '{': LexBracket(TokenType.OpenScope); break;
                    case '}': LexBracket(TokenType.CloseScope); break;

                    case '<': LexOperator(TokenType.LessThan); break;
                    case '>': LexOperator(TokenType.GreaterThan); break;
                    case '+': LexOperator(TokenType.Addition); break;
                    case '-': LexOperator(TokenType.Subtraction); break;
                    case '*': LexOperator(TokenType.Multiplication); break;
                    case '/': LexOperator(TokenType.Division); break;// Also comments
                    case '&': LexOperator(TokenType.BitwiseAnd); break;
                    case '|': LexOperator(TokenType.BitwiseOr); break;
                    case '%': LexOperator(TokenType.Modulo); break;
                    case '!': LexOperator(TokenType.LogicalNot); break;
                    case '=': LexOperator(TokenType.Assignment); break;

                    default:
                    AddToken(c.ToString(), TokenType.Garbage);
                    Next(background: ErrorColor);
                    break;
                }
            }
        }

        if (debug)
        {
            Console.WriteLine("-------- Lexer --------");
        }

        AddToken(string.Empty, TokenType.Eof);

        return tokens.ToArray();
    }

    private void LexWhitespace(char c)
    {
        if (c == '\r')
        {
            Next(display: whitespace ? "\\r" : "", foreground: WhitespaceColor);
        }
        else if (c == '\n')
        {
            line++;
            LineEndings.Add(endIndex, line);
            Next(display: whitespace ? "\\n\n" : "\n", foreground: WhitespaceColor);
        }
        else if (c == '\t')
        {
            Next(display: whitespace ? "»   " : "    ", foreground: WhitespaceColor);
        }
        else
        {
            // Eat all other whitespace
            Next(display: whitespace ? "·" : " ", foreground: WhitespaceColor);
        }
    }

    private void LexOperator(TokenType type)
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
            LexSingleLineComment();
            return;
        }

        AddToken(op, type);
    }

    private void LexSingleLineComment()
    {
        if (debug)
        {
            Console.CursorLeft--;
            Print('/', foreground: CommentColor);
        }

        StringBuilder comment = new("/");

        while (!IsLineBreak(Peek()) && !sourceFile.EndOfStream)
        {
            comment.Append(Next(CommentColor));
        }

        AddToken(comment.ToString(), TokenType.Comment);
    }

    private void LexBracket(TokenType type)
    {
        char c = Next(BracketsColor);

        AddToken(c.ToString(), type);
    }

    private void LexString()
    {
        StringBuilder literal = new();

        // Eat first "
        char c = Next(StringColor);
        literal.Append(c);

        do
        {
            c = Next(StringColor);
            literal.Append(c);
        }
        while (c != '"' && !IsLineBreak(c));

        AddToken(literal.ToString(), TokenType.StringLiteral);
    }

    private void LexIdentifier()
    {
        StringBuilder identifierBuilder = new();
        char c;

        do
        {
            c = Next(IdentifierColor);
            identifierBuilder.Append(c);
        }
        while (char.IsLetterOrDigit(Peek()) && !IsLineBreak(c));

        string identifier = identifierBuilder.ToString().ToLower(CultureInfo.CurrentCulture);

        if (Keywords.TryGetValue(identifier, out TokenType value))
        {
            if (debug)
            {
                // Recolor keywords
                if (!Console.IsOutputRedirected)
                {
                    Console.CursorLeft -= identifier.Length;
                }

                ConsoleColor color = ControlflowKeywords.TryGetValue(identifier, out _) ? ControlflowColor : KeywordColor;
                Print(identifier, foreground: color);
            }

            AddToken(identifierBuilder.ToString(), value);
        }
        else
        {
            AddToken(identifierBuilder.ToString(), TokenType.Identifier);
        }
    }

    private char Peek()
    {
        return (char)sourceFile.Peek();
    }

    private void LexNumber()
    {
        StringBuilder number = new();

        char c;

        do
        {
            c = Next(NumberColor);
            number.Append(c);
        }
        while (char.IsDigit(Peek()));

        AddToken(number.ToString(), TokenType.IntegerLiteral);
    }

    private bool IsLineBreak(char c)
    {
        return c == '\n' || c == '\r';
    }

    private void AddToken(string value, TokenType type)
    {
        tokens.Add(new(value, type, new(file, startIndex, endIndex)));
    }

    private char Next(ConsoleColor foreground = ConsoleColor.White, ConsoleColor background = ConsoleColor.Black, string display = null)
    {
        char c = (char)sourceFile.Read();

        endIndex++;

        if (debug)
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

    private static void Print(char c, ConsoleColor background = ConsoleColor.Black, ConsoleColor foreground = ConsoleColor.White)
    {
        Console.BackgroundColor = background;
        Console.ForegroundColor = foreground;
        Console.Write(c);
        Console.ResetColor();
    }

    private static void Print(string str, ConsoleColor background = ConsoleColor.Black, ConsoleColor foreground = ConsoleColor.White)
    {
        Console.BackgroundColor = background;
        Console.ForegroundColor = foreground;
        Console.Write(str);
        Console.ResetColor();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        sourceFile.Dispose();
    }
}
