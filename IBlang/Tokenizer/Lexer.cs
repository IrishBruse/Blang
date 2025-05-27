namespace IBlang.Tokenizer;

using System.Collections.Generic;
using System.IO;
using System.Text;
using IBlang;
using IBlang.Utility;

public class Lexer(CompilationData data)
{
    public StreamReader Source = null!;
    public string FilePath = "";

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
                    ',' => LexOperator(TokenType.Comma),

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
            _ = Next();
        }
        else if (c == '\n')
        {
            data.Lines.Add(startIndex);
            _ = Next();
        }
        else if (c == '\t')
        {
            _ = Next();
        }
        else
        {
            // Eat all other whitespace
            _ = Next();
        }

        EndTokenRange();
    }

    Token LexOperator(TokenType type)
    {
        char c = Next();
        char p = Peek();

        string op = c.ToString();

        if (c == '<' && p == '=')
        {
            type = TokenType.LessThanEqual;
            op += Next();
        }
        else if (c == '>' && p == '=')
        {
            type = TokenType.GreaterThanEqual;
            op += Next();
        }
        else if (c == '=' && p == '=')
        {
            type = TokenType.EqualEqual;
            op += Next();
        }
        else if (c == '!' && p == '=')
        {
            type = TokenType.NotEqual;
            op += Next();
        }
        else if (c == '&' && p == '&')
        {
            type = TokenType.LogicalAnd;
            op += Next();
        }
        else if (c == '|' && p == '|')
        {
            type = TokenType.LogicalOr;
            op += Next();
        }
        else if (c == '+' && p == '=')
        {
            type = TokenType.AdditionAssignment;
            op += Next();
        }
        else if (c == '-' && p == '=')
        {
            type = TokenType.SubtractionAssignment;
            op += Next();
        }
        else if (c == '*' && p == '=')
        {
            type = TokenType.MultiplicationAssignment;
            op += Next();
        }
        else if (c == '/' && p == '=')
        {
            type = TokenType.DivisionAssignment;
            op += Next();
        }
        else if (c == '%' && p == '=')
        {
            type = TokenType.ModuloAssignment;
            op += Next();
        }
        else if (c == '<' && p == '<')
        {
            type = TokenType.BitwiseShiftLeft;
            op += Next();
        }
        else if (c == '>' && p == '>')
        {
            type = TokenType.BitwiseShiftRight;
            op += Next();
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
        StringBuilder comment = new("/");

        while (!IsLineBreak(Peek()) && !Source.EndOfStream)
        {
            _ = comment.Append(Next());
        }

        return new Token(TokenType.Comment, comment.ToString(), EndTokenRange());
    }

    Token LexMultiLineComment()
    {
        StringBuilder comment = new("/*");

        Next(); // Eat *

        while (!Source.EndOfStream)
        {
            char c = Next();
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
        char c = Next();

        return new Token(type, c.ToString(), EndTokenRange());
    }

    Token LexString()
    {
        StringBuilder literal = new();

        // Eat first "
        _ = Next();

        char c;

        do
        {
            c = Next();
            _ = literal.Append(c);
        }
        while (Peek() != '"' && !IsLineBreak(Peek()));

        _ = Next();

        return new Token(TokenType.StringLiteral, literal.ToString(), EndTokenRange());
    }

    Token LexIdentifier()
    {
        StringBuilder identifierBuilder = new();
        char c;

        do
        {
            c = Next();
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
            c = Next();
            _ = number.Append(c);
        }
        while (char.IsDigit(Peek()));

        return new Token(TokenType.IntegerLiteral, number.ToString(), EndTokenRange());
    }

    static bool IsLineBreak(char c) => c is '\n' or '\r';

    char Next()
    {
        char c = (char)Source.Read();

        endIndex++;

        return c;
    }

    public void StartTokenRange()
    {
        startIndex = endIndex;
    }

    public SourceRange EndTokenRange()
    {
        return new SourceRange(startIndex, endIndex);
    }
}
