namespace IBlang.LexerStage;

public class Lexer : IDisposable
{
    private StreamReader file;
    private int lineNumber = 1;
    private int columnNumber = 1;
    private int index = 1;

    private static readonly Dictionary<string, bool> Keywords = new() {
        { "func", true },
        { "if", true },
        { "else", true },
        { "return", true },
    };

    public Lexer(string file)
    {
        this.file = File.OpenText(file);
    }

    public IEnumerable<Token> GetNextToken()
    {
        while (!file.EndOfStream)
        {
            bool returnedEol = false;

            // Eat all whitespace
            while (char.IsWhiteSpace(PeekChar()))
            {
                char whitespace = NextChar();
                if (file.EndOfStream)
                {
                    yield return new Token(TokenType.Eof, "EOF", new(index - 1, index));
                    yield break;
                }
                else if (whitespace == '\n')
                {
                    if (returnedEol == false)
                    {
                        yield return new Token(TokenType.Eol, "EOL", new(index - 1, index));
                        returnedEol = true;
                    }
                }
            }

            int start = index;

            char c = PeekChar();

            yield return c switch
            {
                '(' => new Token(TokenType.OpenParenthesis, NextChar().ToString(), new(start, index)),
                ')' => new Token(TokenType.CloseParenthesis, NextChar().ToString(), new(start, index)),
                '[' => new Token(TokenType.OpenBracket, NextChar().ToString(), new(start, index)),
                ']' => new Token(TokenType.CloseBracket, NextChar().ToString(), new(start, index)),
                '{' => new Token(TokenType.OpenScope, NextChar().ToString(), new(start, index)),
                '}' => new Token(TokenType.CloseScope, NextChar().ToString(), new(start, index)),

                '.' => new Token(TokenType.Dot, NextChar().ToString(), new(start, index)),
                ',' => new Token(TokenType.Comma, NextChar().ToString(), new(start, index)),

                '<' => new Token(TokenType.LessThan, NextChar().ToString(), new(start, index)),
                '>' => new Token(TokenType.GreaterThan, NextChar().ToString(), new(start, index)),
                '~' => new Token(TokenType.Tilda, NextChar().ToString(), new(start, index)),
                ':' => new Token(TokenType.Colon, NextChar().ToString(), new(start, index)),
                '?' => new Token(TokenType.Question, NextChar().ToString(), new(start, index)),

                '+' => new Token(TokenType.Plus, NextChar().ToString(), new(start, index)),
                '-' => new Token(TokenType.Minus, NextChar().ToString(), new(start, index)),
                '*' => new Token(TokenType.Multiply, NextChar().ToString(), new(start, index)),
                '/' => new Token(TokenType.Divide, NextChar().ToString(), new(start, index)),
                '^' => new Token(TokenType.Caret, NextChar().ToString(), new(start, index)),
                '&' => new Token(TokenType.And, NextChar().ToString(), new(start, index)),
                '|' => new Token(TokenType.Pipe, NextChar().ToString(), new(start, index)),
                '%' => new Token(TokenType.Module, NextChar().ToString(), new(start, index)),
                '!' => new Token(TokenType.Exclemation, NextChar().ToString(), new(start, index)),
                '=' => new Token(TokenType.Equal, NextChar().ToString(), new(start, index)),

                '$' => new Token(TokenType.Dollar, NextChar().ToString(), new(start, index)),
                ';' => new Token(TokenType.Semicolon, NextChar().ToString(), new(start, index)),

                '"' => LexString(),
                '\'' => LexChar(),

                _ => LexIdentifier(),
            };
        }
    }

    private Token LexString()
    {
        int start = index;

        Assert('"' == NextChar());// Eat the quote

        string stringLiteral = "";
        while (PeekChar() is not '"' and not '\n')
        {
            stringLiteral += NextChar();
        }

        Assert('"' == NextChar());// Eat the quote

        return new Token(TokenType.StringLiteral, stringLiteral, new(start, index));
    }

    private Token LexChar()
    {
        int start = index;

        char charLiteral;
        Assert('\'' == NextChar());// Eat the quote
        if (PeekChar() == '\\')// Handle Escape
        {
            Assert('\'' == NextChar());// Eat the \

            char escapeCode = NextChar();
            switch (escapeCode)
            {
                case 'a': charLiteral = '\a'; break;
                case 'b': charLiteral = '\b'; break;
                case 't': charLiteral = '\t'; break;
                case 'r': charLiteral = '\r'; break;
                case 'v': charLiteral = '\v'; break;
                case 'f': charLiteral = '\f'; break;
                case 'n': charLiteral = '\n'; break;
                case '0': charLiteral = '\0'; break;
                case '\\': charLiteral = '\\'; break;
                default: return new Token(TokenType.CharLiteral, string.Empty, new(start, index));
            };
        }
        else
        {
            charLiteral = NextChar();
        }
        Assert('\'' == NextChar());// Eat the quote

        return new Token(TokenType.CharLiteral, charLiteral.ToString(), new(start, index));
    }

    private Token LexIdentifier()
    {
        int start = index;

        string identifier = "";
        while (char.IsLetter(PeekChar()))
        {
            identifier += NextChar();
        }

        _ = Keywords.TryGetValue(identifier, out bool isKeyword);
        if (isKeyword)
        {
            return new Token(TokenType.Keyword, identifier, new(start, index));
        }
        else
        {
            return new Token(TokenType.Identifier, identifier, new(start, index));
        }
    }

    private char NextChar()
    {
        char c = (char)file.Read();
        if (c == '\n')
        {
            lineNumber++;
            columnNumber = 1;
        }
        else
        {
            columnNumber++;
        }
        index++;
        return c;
    }

    private char PeekChar()
    {
        return (char)file.Peek();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        file.Close();
    }
}
