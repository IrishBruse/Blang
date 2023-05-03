namespace IBlang;

using System;
using System.Globalization;
using System.IO;
using System.Text;

public class Lexer
{
    private static readonly HashSet<string> Keywords = new()
    {
        { "func" },

        { "if" },
        { "else" },
        { "return" },

        { "true" },
        { "false" },
    };

    private static readonly HashSet<string> ControlflowKeywords = new()
    {
        { "if" },
        { "else" },
        { "return" },
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

    public Lexer(StreamReader sourceFile)
    {
        this.sourceFile = sourceFile;
        // PrintConsoleColors();
    }

    private static void PrintConsoleColors()
    {
        Console.WriteLine();
        Print("Black\n", foreground: ConsoleColor.Black);
        Print("DarkBlue\n", foreground: ConsoleColor.DarkBlue);
        Print("DarkGreen\n", foreground: ConsoleColor.DarkGreen);
        Print("DarkCyan\n", foreground: ConsoleColor.DarkCyan);
        Print("DarkRed\n", foreground: ConsoleColor.DarkRed);
        Print("DarkMagenta\n", foreground: ConsoleColor.DarkMagenta);
        Print("DarkYellow\n", foreground: ConsoleColor.DarkYellow);
        Print("Gray\n", foreground: ConsoleColor.Gray);
        Print("DarkGray\n", foreground: ConsoleColor.DarkGray);
        Print("Blue\n", foreground: ConsoleColor.Blue);
        Print("Green\n", foreground: ConsoleColor.Green);
        Print("Cyan\n", foreground: ConsoleColor.Cyan);
        Print("Red\n", foreground: ConsoleColor.Red);
        Print("Magenta\n", foreground: ConsoleColor.Magenta);
        Print("Yellow\n", foreground: ConsoleColor.Yellow);
        Print("White\n", foreground: ConsoleColor.White);
        Print("     \n", ConsoleColor.Black);
        Print("     \n", ConsoleColor.DarkBlue);
        Print("     \n", ConsoleColor.DarkGreen);
        Print("     \n", ConsoleColor.DarkCyan);
        Print("     \n", ConsoleColor.DarkRed);
        Print("     \n", ConsoleColor.DarkMagenta);
        Print("     \n", ConsoleColor.DarkYellow);
        Print("     \n", ConsoleColor.Gray);
        Print("     \n", ConsoleColor.DarkGray);
        Print("     \n", ConsoleColor.Blue);
        Print("     \n", ConsoleColor.Green);
        Print("     \n", ConsoleColor.Cyan);
        Print("     \n", ConsoleColor.Red);
        Print("     \n", ConsoleColor.Magenta);
        Print("     \n", ConsoleColor.Yellow);
        Print("     \n", ConsoleColor.White);
        Console.WriteLine();
    }

    public Token[] Lex()
    {
        Console.WriteLine("-------- Lexed code --------");

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
                    case '!': LexOperator(TokenType.LogicalNot); break;
                    case '=': LexOperator(TokenType.Assignment); break;

                    default: Next(background: ErrorColor); break;
                }
            }
        }

        Console.WriteLine("-------- End Lexed code --------");

        tokens.Add(new(string.Empty, TokenType.Eof, 0, 0));

        return tokens.ToArray();
    }

    private void LexOperator(TokenType singleType)
    {
        char c = Next(OperatorColor);
        char p = Peek();

        if (c == '<' && p == '=')
        {
            singleType = TokenType.LessThanEqual;
            Next(OperatorColor);
        }
        else if (c == '>' && p == '=')
        {
            singleType = TokenType.GreaterThanEqual;
            Next(OperatorColor);
        }
        else if (c == '=' && p == '=')
        {
            singleType = TokenType.EqualEqual;
            Next(OperatorColor);
        }
        else if (c == '!' && p == '=')
        {
            singleType = TokenType.NotEqual;
            Next(OperatorColor);
        }
        else if (c == '&' && p == '&')
        {
            singleType = TokenType.LogicalAnd;
            Next(OperatorColor);
        }
        else if (c == '|' && p == '|')
        {
            singleType = TokenType.LogicalOr;
            Next(OperatorColor);
        }
        else if (c == '+' && p == '=')
        {
            singleType = TokenType.AdditionAssignment;
            Next(OperatorColor);
        }
        else if (c == '-' && p == '=')
        {
            singleType = TokenType.SubtractionAssignment;
            Next(OperatorColor);
        }
        else if (c == '*' && p == '=')
        {
            singleType = TokenType.MultiplicationAssignment;
            Next(OperatorColor);
        }
        else if (c == '/' && p == '=')
        {
            singleType = TokenType.DivisionAssignment;
            Next(OperatorColor);
        }
        else if (c == '%' && p == '=')
        {
            singleType = TokenType.ModuloAssignment;
            Next(OperatorColor);
        }
        else if (c == '<' && p == '<')
        {
            singleType = TokenType.BitwiseShiftLeft;
            Next(OperatorColor);
        }
        else if (c == '>' && p == '>')
        {
            singleType = TokenType.BitwiseShiftRight;
            Next(OperatorColor);
        }
        else if (c == '/' && p == '/') // Single line comment
        {
            Console.CursorLeft--;
            Print('/', foreground: CommentColor);

            while (c != '\n')
            {
                c = Next(CommentColor);
            }

            // We didnt actually process any opertor just comments
            return;
        }

        tokens.Add(new(c.ToString(), singleType, 0, 0));
    }

    private void LexBracket(TokenType type)
    {
        char c = Next(BracketsColor);

        tokens.Add(new(c.ToString(), type, 0, 0));
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

        tokens.Add(new(literal.ToString(), TokenType.StringLiteral, 0, 0));
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

        if (Keywords.Contains(identifier))
        {
            // Recolor keywords
            Console.CursorLeft -= identifier.Length;
            ConsoleColor color = ControlflowKeywords.Contains(identifier) ? ControlflowColor : KeywordColor;
            Print(identifier, foreground: color);
        }

        tokens.Add(new(identifierBuilder.ToString(), TokenType.Identifier, 0, 0));
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

        tokens.Add(new(number.ToString(), TokenType.IntegerLiteral, 0, 0));
    }

    private char Next(ConsoleColor foreground = ConsoleColor.White, ConsoleColor background = ConsoleColor.Black, string display = null)
    {
        char c = (char)sourceFile.Read();

        if (display != null)
        {
            Print(display, background, foreground);
        }
        else
        {
            Print(c, background, foreground);
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
}
