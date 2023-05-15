namespace IBlang;

using System;
using System.Globalization;
using System.IO;
using System.Text;

public class Lexer : IDisposable
{
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
    private List<Token> tokens = new();
    private readonly bool debug;
    private List<int> lineEndings = new() { 0 };
    private int index;

    public Lexer(StreamReader sourceFile, bool debug = false)
    {
        this.sourceFile = sourceFile;
        this.debug = debug;
    }

    public Lexer(string sourceText, bool debug = false)
    {
        MemoryStream stream = new();
        StreamWriter writer = new(stream);
        writer.Write(sourceText);
        writer.Flush();
        stream.Position = 0;

        sourceFile = new StreamReader(stream);
        this.debug = debug;
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

            if (char.IsWhiteSpace(c))
            {
                if (c == '\r')
                {
                    Next(display: string.Empty);
                }
                else if (c == '\n')
                {
                    Print("|" + index + "|");
                    lineEndings.Add(index);
                    Next(display: "\\n\n", foreground: WhitespaceColor);
                }
                else if (c == '\t')
                {
                    Next(display: "»   ", foreground: WhitespaceColor);
                }
                else
                {
                    // Eat all other whitespace
                    Next(display: "·", foreground: WhitespaceColor);
                }
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
                    AddToken(new(c.ToString(), TokenType.Garbage, 0, 0));
                    Next(background: ErrorColor);
                    break;
                }
            }
        }

        if (debug)
        {
            Console.WriteLine("-------- Lexer --------");
        }

        AddToken(new(string.Empty, TokenType.Eof, 0, 0));

        return tokens.ToArray();
    }

    private void LexOperator(TokenType type)
    {
        char c = Next(OperatorColor);
        char p = Peek();

        if (c == '<' && p == '=')
        {
            type = TokenType.LessThanEqual;
            Next(OperatorColor);
        }
        else if (c == '>' && p == '=')
        {
            type = TokenType.GreaterThanEqual;
            Next(OperatorColor);
        }
        else if (c == '=' && p == '=')
        {
            type = TokenType.EqualEqual;
            Next(OperatorColor);
        }
        else if (c == '!' && p == '=')
        {
            type = TokenType.NotEqual;
            Next(OperatorColor);
        }
        else if (c == '&' && p == '&')
        {
            type = TokenType.LogicalAnd;
            Next(OperatorColor);
        }
        else if (c == '|' && p == '|')
        {
            type = TokenType.LogicalOr;
            Next(OperatorColor);
        }
        else if (c == '+' && p == '=')
        {
            type = TokenType.AdditionAssignment;
            Next(OperatorColor);
        }
        else if (c == '-' && p == '=')
        {
            type = TokenType.SubtractionAssignment;
            Next(OperatorColor);
        }
        else if (c == '*' && p == '=')
        {
            type = TokenType.MultiplicationAssignment;
            Next(OperatorColor);
        }
        else if (c == '/' && p == '=')
        {
            type = TokenType.DivisionAssignment;
            Next(OperatorColor);
        }
        else if (c == '%' && p == '=')
        {
            type = TokenType.ModuloAssignment;
            Next(OperatorColor);
        }
        else if (c == '<' && p == '<')
        {
            type = TokenType.BitwiseShiftLeft;
            Next(OperatorColor);
        }
        else if (c == '>' && p == '>')
        {
            type = TokenType.BitwiseShiftRight;
            Next(OperatorColor);
        }
        else if (c == '/' && p == '/') // Single line comment
        {
            if (debug)
            {
                Console.CursorLeft--;
                Print('/', foreground: CommentColor);
            }

            while (Peek() != '\n' && !sourceFile.EndOfStream)
            {
                Next(CommentColor);
            }

            // We didnt actually process any opertor just comments
            return;
        }

        AddToken(new(c.ToString(), type, 0, 0));
    }
    private void LexBracket(TokenType type)
    {
        char c = Next(BracketsColor);

        AddToken(new(c.ToString(), type, 0, 0));
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
        while (c != '"' && c != '\n');

        AddToken(new(literal.ToString(), TokenType.StringLiteral, 0, 0));
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
        while (char.IsLetterOrDigit(Peek()) && c != '\n');

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

            AddToken(new(identifierBuilder.ToString(), value, 0, 0));
        }
        else
        {
            AddToken(new(identifierBuilder.ToString(), TokenType.Identifier, 0, 0));
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

        AddToken(new(number.ToString(), TokenType.IntegerLiteral, 0, index));
    }


    private void AddToken(Token token)
    {
        tokens.Add(token);
    }

    private char Next(ConsoleColor foreground = ConsoleColor.White, ConsoleColor background = ConsoleColor.Black, string display = null)
    {
        char c = (char)sourceFile.Read();

        index++;

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
