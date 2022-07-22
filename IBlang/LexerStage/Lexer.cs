namespace IBlang.LexerStage;

public class Lexer
{
    private StreamReader file;
    private int lineNumber = 1;
    private int columnNumber = 1;

    private int index = 1;
    private Context ctx;
    private static readonly Dictionary<string, TokenType> Keywords = new() {
        { "func",   TokenType.Keyword_Func },
        { "if",     TokenType.Keyword_If },
        { "else",   TokenType.Keyword_Else },
        { "return", TokenType.Keyword_Return },
    };

    public Lexer(Context ctx)
    {
        this.ctx = ctx;
        file = File.OpenText(ctx.Files[0]);
    }

    public Token[] Lex()
    {
        List<Token> tokens = new();

        while (!file.EndOfStream)
        {
            // Eat all whitespace
            while (char.IsWhiteSpace(PeekChar()))
            {
                _ = NextChar();
                if (file.EndOfStream)
                {
                    tokens.Add(new Token(TokenType.Eof, "EOF", new(index - 1, index)));

                    file.Close();
                    return tokens.ToArray();
                }
            }

            int start = index;

            char c = PeekChar();

            Token token = c switch
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

                char digit when char.IsDigit(digit) => LexNumber(),
                char letter when char.IsLetter(letter) => LexIdentifierOrKeyword(),

                _ => new Token(TokenType.Garbage, NextChar().ToString(), new(start, index)),
            };

            tokens.Add(token);
        }

        file.Close();

        return tokens.ToArray();
    }

    private Token LexNumber()
    {
        int start = index;

        string numberLiteral = "";
        while (char.IsDigit(PeekChar()))
        {
            numberLiteral += NextChar();
        }

        return new Token(TokenType.NumberLiteral, numberLiteral, new(start, index));

    }

    private Token LexString()
    {
        int start = index;

        Log.Assert('"' == NextChar());// Eat the quote

        string stringLiteral = "";
        while (PeekChar() is not '"' and not '\n')
        {
            stringLiteral += NextChar();
        }

        Log.Assert('"' == NextChar());// Eat the quote

        return new Token(TokenType.StringLiteral, stringLiteral, new(start, index));
    }

    private Token LexChar()
    {
        int start = index;

        char charLiteral;
        Log.Assert('\'' == NextChar());// Eat the quote
        if (PeekChar() == '\\')// Handle Escape
        {
            Log.Assert('\'' == NextChar());// Eat the \

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
        Log.Assert('\'' == NextChar());// Eat the quote

        return new Token(TokenType.CharLiteral, charLiteral.ToString(), new(start, index));
    }

    private Token LexIdentifierOrKeyword()
    {
        int start = index;

        string identifier = "";
        while (char.IsLetter(PeekChar()))
        {
            identifier += NextChar();
        }

        if (Keywords.TryGetValue(identifier, out TokenType type))
        {
            return new Token(type, identifier, new(start, index));
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
}
