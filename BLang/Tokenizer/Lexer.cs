namespace BLang.Tokenizer;

using System.Collections.Generic;
using System.IO;
using System.Text;
using BLang.Utility;

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
            StartTokenRange();

            char c = Next();

            if (char.IsWhiteSpace(c))
            {
                EatWhitespace(c);
            }
            else if (char.IsLetter(c))
            {
                yield return LexIdentifier(c);
            }
            else if (char.IsNumber(c))
            {
                yield return LexNumber(c);
            }
            else
            {
                char p = Peek();

                if (c == '/' && p == '/')
                {
                    LexSingleLineComment();
                    continue;
                }
                else if (c == '/' && p == '*')
                {
                    LexMultiLineComment();
                    continue;
                }

                yield return c switch
                {
                    '"' => LexString(),

                    '[' => LexBracket(c, TokenType.OpenBracket),
                    ']' => LexBracket(c, TokenType.CloseBracket),

                    '(' => LexBracket(c, TokenType.OpenParenthesis),
                    ')' => LexBracket(c, TokenType.CloseParenthesis),

                    '{' => LexBracket(c, TokenType.OpenScope),
                    '}' => LexBracket(c, TokenType.CloseScope),

                    '<' => LexOperator(c, TokenType.LessThan),
                    '>' => LexOperator(c, TokenType.GreaterThan),

                    '+' => LexOperator(c, TokenType.Addition),
                    '-' => LexOperator(c, TokenType.Subtraction),
                    '*' => LexOperator(c, TokenType.Multiplication),
                    '/' => LexOperator(c, TokenType.Division),
                    '&' => LexOperator(c, TokenType.BitwiseAnd),
                    '|' => LexOperator(c, TokenType.BitwiseOr),
                    '%' => LexOperator(c, TokenType.Modulo),
                    '!' => LexOperator(c, TokenType.LogicalNot),
                    '=' => LexOperator(c, TokenType.Assignment),

                    ';' => LexOperator(c, TokenType.Semicolon),
                    ',' => LexOperator(c, TokenType.Comma),

                    _ => new Token(TokenType.Garbage, c.ToString(), EndTokenRange())
                };
            }
        }

        Source.Close();

        yield return new Token(TokenType.Eof, string.Empty, EndTokenRange());
    }

    void EatWhitespace(char c)
    {
        while (char.IsWhiteSpace(Peek()))
        {
            if (c == '\n')
            {
                _ = Next();
            }
            else
            {
                // Eat all other whitespace
                _ = Next();
            }
        }

        EndTokenRange();
    }

    Token LexOperator(char c, TokenType type)
    {
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

    Token LexBracket(char c, TokenType type)
    {
        return new Token(type, c.ToString(), EndTokenRange());
    }

    Token LexString()
    {
        StringBuilder literal = new();

        char c;

        while (Peek() != '"' && !IsLineBreak(Peek()))
        {
            c = Next();
            _ = literal.Append(c);
        }

        _ = Next();

        return new Token(TokenType.StringLiteral, literal.ToString(), EndTokenRange());
    }

    Token LexIdentifier(char c)
    {
        StringBuilder identifierBuilder = new(c.ToString());

        while (char.IsLetterOrDigit(Peek()) && !IsLineBreak(Peek()))
        {
            identifierBuilder.Append(Next());
        }

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

    Token LexNumber(char c)
    {
        StringBuilder number = new(c.ToString());

        while (char.IsDigit(Peek()))
        {
            c = Next();
            number.Append(c);
        }

        return new Token(TokenType.IntegerLiteral, number.ToString(), EndTokenRange());
    }

    static bool IsLineBreak(char c) => c == '\n' || c == '\r';

    char Next()
    {
        char c = (char)Source.Read();

        endIndex++;

        if (IsLineBreak(c))
        {
            data.Lines.Add(endIndex);
        }

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
