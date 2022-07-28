namespace IBlang.LexerStage;
public class Lexer
{
    private readonly StreamReader file;

    private int index = 1;
    private int oldIndex = 1;
    private readonly Context ctx;

    private static readonly Dictionary<string, TokenType> Keywords = new() {
        { "func",   TokenType.KeywordFunc },
        { "if",     TokenType.KeywordIf },
        { "else",   TokenType.KeywordElse },
        { "return", TokenType.KeywordReturn },
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

            char c = NextChar();

            if (PeekChar() == '/')
            {
                while (PeekChar() != '\n')
                {
                    _ = NextChar();
                }
                continue;
            }

            Token token = c switch
            {
                '(' => new Token(TokenType.OpenParenthesis, c.ToString(), new(start, index)),
                ')' => new Token(TokenType.CloseParenthesis, c.ToString(), new(start, index)),
                '[' => new Token(TokenType.OpenBracket, c.ToString(), new(start, index)),
                ']' => new Token(TokenType.CloseBracket, c.ToString(), new(start, index)),
                '{' => new Token(TokenType.OpenScope, c.ToString(), new(start, index)),
                '}' => new Token(TokenType.CloseScope, c.ToString(), new(start, index)),

                '.' => new Token(TokenType.Dot, c.ToString(), new(start, index)),
                ',' => new Token(TokenType.Comma, c.ToString(), new(start, index)),

                '<' => LexBinaryOperator(TokenType.LessThan, start, c),
                '>' => LexBinaryOperator(TokenType.GreaterThan, start, c),
                '+' => LexBinaryOperator(TokenType.Addition, start, c),
                '-' => LexBinaryOperator(TokenType.Subtraction, start, c),
                '*' => LexBinaryOperator(TokenType.Multiplication, start, c),
                '/' => LexBinaryOperator(TokenType.Division, start, c),
                '&' => LexBinaryOperator(TokenType.BitwiseAnd, start, c),
                '|' => LexBinaryOperator(TokenType.BitwiseOr, start, c),
                '!' => LexBinaryOperator(TokenType.LogicalNot, start, c),
                '=' => LexBinaryOperator(TokenType.Assignment, start, c),

                '"' => LexString(),
                '\'' => LexChar(),

                char digit when char.IsDigit(digit) => LexNumber(c.ToString()),
                char letter when char.IsLetter(letter) => LexIdentifierOrKeyword(c.ToString()),

                _ => new Token(TokenType.Garbage, c.ToString(), new(start, index)),
            };

            tokens.Add(token);
        }

        file.Close();

        return tokens.ToArray();
    }

    private Token LexBinaryOperator(TokenType token, int start, char c)
    {
        TokenType type = token;
        char p = PeekChar();

        if (c == '<' && p == '=')
        {
            type = TokenType.LessThanEqual;
            _ = NextChar();
        }
        else if (c == '<' && p == '=')
        {
            type = TokenType.GreaterThanEqual;
            _ = NextChar();
        }
        else if (c == '=' && p == '=')
        {
            type = TokenType.EqualEqual;
            _ = NextChar();
        }
        else if (c == '!' && p == '=')
        {
            type = TokenType.NotEqual;
            _ = NextChar();
        }
        else if (c == '&' && p == '&')
        {
            type = TokenType.LogicalAnd;
            _ = NextChar();
        }
        else if (c == '|' && p == '|')
        {
            type = TokenType.LogicalOr;
            _ = NextChar();
        }
        else if (c == '+' && p == '=')
        {
            type = TokenType.AdditionAssignment;
            _ = NextChar();
        }
        else if (c == '-' && p == '=')
        {
            type = TokenType.SubtractionAssignment;
            _ = NextChar();
        }
        else if (c == '*' && p == '=')
        {
            type = TokenType.MultiplicationAssignment;
            _ = NextChar();
        }
        else if (c == '/' && p == '=')
        {
            type = TokenType.DivisionAssignment;
            _ = NextChar();
        }
        else if (c == '%' && p == '=')
        {
            type = TokenType.ModuloAssignment;
            _ = NextChar();
        }
        else if (c == '<' && p == '<')
        {
            type = TokenType.BitwiseShiftLeft;
            _ = NextChar();
        }
        else if (c == '>' && p == '>')
        {
            type = TokenType.BitwiseShiftRight;
            _ = NextChar();
        }

        Span span = new(start, index);
        return new Token(type, $"{c}{p}", span);
    }

    private Token LexNumber(string c)
    {
        int start = index;

        string numberLiteral = c;
        while (char.IsDigit(PeekChar()))
        {
            numberLiteral += NextChar();
        }

        return new Token(TokenType.IntegerLiteral, numberLiteral, new(start, index));

    }

    private Token LexString()
    {
        int start = index;

        string stringLiteral = "";
        while (PeekChar() is not '"' and not '\n')
        {
            stringLiteral += NextChar();
        }

        EatChar('"');// Eat the quote

        return new Token(TokenType.StringLiteral, stringLiteral, new(start, index));
    }

    private Token LexChar()
    {
        int start = index;

        char charLiteral;

        if (PeekChar() == '\\')// Handle Escape
        {
            EatChar('\'');

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
        EatChar('\'');// Eat the quote

        return new Token(TokenType.CharLiteral, charLiteral.ToString(), new(start, index));
    }

    private void EatChar(char c) => Log.Assert(c == NextChar());// Eat the \

    private Token LexIdentifierOrKeyword(string c)
    {
        int start = index;

        string identifier = c;
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
            ctx.LineSpans.Add(new(oldIndex, index));
            oldIndex = index;
        }
        index++;
        return c;
    }

    private char PeekChar() => (char)file.Peek();
}
