namespace BLang.Tokenizer;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using BLang.Utility;

public class Lexer(CompilerContext data)
{
    public PeekableStream Source { get; set; } = null!;
    public string FilePath { get; set; } = "";

    private int endIndex;
    private int startIndex;

    public IEnumerator<Token> Lex(Stream fileStream, string path)
    {
        Source = new(fileStream);
        FilePath = path;
        while (!Source.EndOfStream)
        {
            StartTokenRange();

            char c = Next();
            char p = Peek();

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
            else if (c == '/' && p == '/')
            {
                _ = LexSingleLineComment();
                continue;
            }
            else if (c == '/' && p == '*')
            {
                _ = LexMultiLineComment();
                continue;
            }
            else if (c == '"')
            {
                yield return LexString();
            }
            else
            {
                yield return LexOperator(c);
            }
        }

        Source.Close();

        yield return new Token(TokenType.Eof, string.Empty, EndTokenRange());
    }

    private void EatWhitespace(char c)
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

        _ = EndTokenRange();
    }

    private static readonly Dictionary<string, TokenType> OperatorMap = new()
    {
        ["<<="] = TokenType.BitwiseShiftLeftAssignment,
        [">>="] = TokenType.BitwiseShiftRightAssignment,

        ["-="] = TokenType.SubtractionAssignment,
        ["=-"] = TokenType.SubtractionAssignment,

        ["+="] = TokenType.AdditionAssignment,
        ["=+"] = TokenType.AdditionAssignment,

        ["*="] = TokenType.MultiplicationAssignment,
        ["/="] = TokenType.DivisionAssignment,
        ["%="] = TokenType.ModuloAssignment,
        ["|="] = TokenType.BitwiseOrAssignment,

        ["<<"] = TokenType.BitwiseShiftLeft,
        [">>"] = TokenType.BitwiseShiftRight,
        ["<="] = TokenType.LessThanEqual,
        [">="] = TokenType.GreaterThanEqual,
        ["=="] = TokenType.EqualEqual,
        ["!="] = TokenType.NotEqual,
        ["&&"] = TokenType.LogicalAnd,
        ["||"] = TokenType.LogicalOr,

        ["++"] = TokenType.Increment,
        ["--"] = TokenType.Decrement,

        ["<"] = TokenType.LessThan,
        [">"] = TokenType.GreaterThan,
        ["+"] = TokenType.Addition,
        ["-"] = TokenType.Subtraction,
        ["*"] = TokenType.Multiplication,
        ["/"] = TokenType.Division,
        ["&"] = TokenType.BitwiseAnd,
        ["|"] = TokenType.BitwiseOr,
        ["%"] = TokenType.Modulo,
        ["!"] = TokenType.LogicalNot,

        ["="] = TokenType.Assignment,
        [";"] = TokenType.Semicolon,
        [","] = TokenType.Comma,

        ["["] = TokenType.OpenBracket,
        ["]"] = TokenType.CloseBracket,

        ["("] = TokenType.OpenParenthesis,
        [")"] = TokenType.CloseParenthesis,

        ["{"] = TokenType.OpenScope,
        ["}"] = TokenType.CloseScope,
    };

    private Token LexOperator(char c)
    {
        // Look ahead up to 2 more characters without consuming the stream
        char p1 = Peek();
        char p2 = Peek(1);

        string threeCharOp = $"{c}{p1}{p2}";

        if (OperatorMap.TryGetValue(threeCharOp, out TokenType type3))
        {
            _ = Next(); // consume p1
            _ = Next(); // consume p2
            return new Token(type3, threeCharOp, EndTokenRange());
        }

        // Try two-character operator
        string twoCharOp = $"{c}{p1}";
        if (OperatorMap.TryGetValue(twoCharOp, out TokenType type2))
        {
            _ = Next(); // consume p1
            return new Token(type2, twoCharOp, EndTokenRange());
        }

        // Try single-character operator
        string oneCharOp = c.ToString();
        if (OperatorMap.TryGetValue(oneCharOp, out TokenType type1))
        {
            return new Token(type1, oneCharOp, EndTokenRange());
        }

        return new Token(TokenType.Garbage, oneCharOp, EndTokenRange());
    }

    private Token LexSingleLineComment()
    {
        StringBuilder comment = new("/");

        while (!IsLineBreak(Peek()) && !Source.EndOfStream)
        {
            _ = comment.Append(Next());
        }


        return new Token(TokenType.Comment, comment.ToString(), EndTokenRange());
    }

    private Token LexMultiLineComment()
    {
        StringBuilder comment = new("/*");

        _ = Next(); // Eat *

        while (!Source.EndOfStream)
        {
            char c = Next();
            _ = c == '\n' ? comment.Append("\\n") : comment.Append(c);

            if (c == '*' && Peek() == '/')
            {
                _ = comment.Append(Next());
                break;
            }
        }

        return new Token(TokenType.Comment, comment.ToString(), EndTokenRange());
    }

    private Token LexBracket(char c, TokenType type)
    {
        return new Token(type, c.ToString(), EndTokenRange());
    }

    private Token LexString()
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

    private Token LexIdentifier(char c)
    {
        StringBuilder identifierBuilder = new(c.ToString());

        while (char.IsLetterOrDigit(Peek()) || (Peek() == '_' && !IsLineBreak(Peek())))
        {
            _ = identifierBuilder.Append(Next());
        }

        string identifier = identifierBuilder.ToString();
        return identifier switch
        {
            "extrn" => new Token(TokenType.ExternKeyword, identifier, EndTokenRange()),
            "if" => new Token(TokenType.IfKeyword, identifier, EndTokenRange()),
            "else" => new Token(TokenType.ElseKeyword, identifier, EndTokenRange()),
            "while" => new Token(TokenType.WhileKeyword, identifier, EndTokenRange()),
            "auto" => new Token(TokenType.AutoKeyword, identifier, EndTokenRange()),
            "switch" => new Token(TokenType.SwitchKeyword, identifier, EndTokenRange()),
            "case" => new Token(TokenType.CaseKeyword, identifier, EndTokenRange()),
            "break" => new Token(TokenType.BreakKeyword, identifier, EndTokenRange()),
            _ => new Token(TokenType.Identifier, identifier, EndTokenRange()),
        };
    }

    private char Peek(int offset = 0)
    {
        return (char)Source.PeekChar(offset);
    }

    private Token LexNumber(char c)
    {
        StringBuilder number = new(c.ToString());

        while (char.IsNumber(Peek()))
        {
            c = Next();
            _ = number.Append(c);
        }

        return new Token(TokenType.IntegerLiteral, number.ToString(), EndTokenRange());
    }

    private static bool IsLineBreak(char c)
    {
        return c is '\n' or '\r';
    }

    private char Next()
    {
        char c = (char)Source.ReadChar();

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
