namespace BLang.Tokenizer;

using BLang.Utility;

public record Token(TokenType TokenType, string Content, SourceRange Range)
{
    public override string ToString()
    {
        return $"{Range.Start,4}-{Range.End,-4} {TokenType,-18} {Content}";
    }
}

public enum TokenType
{
    // Misc
    Eof = -2,
    Garbage = -1,

    None = 0,

    Comment,

    Identifier,

    IntegerLiteral,
    FloatLiteral,
    StringLiteral,
    CharLiteral,

    // Bracket Pairs

    /// <summary> ( </summary>
    OpenParenthesis,
    /// <summary> ) </summary>
    CloseParenthesis,
    /// <summary> [ </summary>
    OpenBracket,
    /// <summary> ] </summary>
    CloseBracket,
    /// <summary> { </summary>
    OpenScope,
    /// <summary> } </summary>
    CloseScope,

    // Punctuation
    Dot,
    Comma,

    // Arithmetic
    Addition,
    Subtraction,
    Multiplication,
    Division,
    Modulo,

    // Relational
    LessThan,
    GreaterThan,
    LessThanEqual,
    GreaterThanEqual,
    EqualEqual,
    NotEqual,

    // Logical
    LogicalAnd,
    LogicalOr,
    LogicalNot,

    // Assignment
    Assignment,
    AdditionAssignment,
    SubtractionAssignment,
    MultiplicationAssignment,
    DivisionAssignment,
    ModuloAssignment,

    // Bitwise
    BitwiseComplement,
    BitwiseAnd,
    BitwiseOr,
    BitwiseXOr,
    BitwiseShiftLeft,
    BitwiseShiftRight,

    Semicolon,

    // Keywords
    ExternKeyword,
    IfKeyword,
    WhileKeyword,
    AutoKeyword
}

public static class Extensions
{
    public static bool IsEnd(this TokenType t)
    {
        return t == TokenType.Garbage || t == TokenType.Eof;
    }

    public static bool IsKeyword(this TokenType t)
    {
        return t == TokenType.ExternKeyword ||
            t == TokenType.IfKeyword ||
            t == TokenType.WhileKeyword ||
            t == TokenType.AutoKeyword;
    }

    public static string ToCharString(this TokenType t)
    {
        return t switch
        {
            TokenType.Semicolon => ";",
            TokenType.Comma => ",",

            TokenType.Addition => "+",
            TokenType.Subtraction => "-",
            TokenType.Multiplication => "*",
            TokenType.Division => "/",
            TokenType.Modulo => "%",

            TokenType.OpenScope => "{",
            TokenType.CloseScope => "}",

            TokenType.OpenParenthesis => "(",
            TokenType.CloseParenthesis => ")",

            TokenType.OpenBracket => "[",
            TokenType.CloseBracket => "]",

            TokenType.LessThan => "<",
            TokenType.LessThanEqual => "<=",
            TokenType.GreaterThan => ">",
            TokenType.GreaterThanEqual => ">=",
            TokenType.EqualEqual => "==",
            TokenType.NotEqual => "!=",
            _ => t.ToString(),
        };
    }

}
