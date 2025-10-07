namespace BLang.Tokenizer;

using BLang.Exceptions;
using BLang.Utility;

public record Token(TokenType TokenType, string Content, SourceRange Range)
{
    public override string ToString()
    {
        return $"{TokenType,-18} {Content}";
    }

    public int ToInteger()
    {
        if (!int.TryParse(Content, out int number))
        {
            throw new InvalidTokenException(Content);
        }
        return number;
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
    Addition = 15,
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


    /// <summary> ++ </summary>
    Increment,
    /// <summary> -- </summary>
    Decrement,

    // Bitwise
    BitwiseComplement,
    BitwiseAnd,
    BitwiseOr,
    BitwiseXOr,
    BitwiseShiftLeft,
    BitwiseShiftRight,


    // Assignment
    Assignment,
    AdditionAssignment,
    SubtractionAssignment,
    MultiplicationAssignment,
    DivisionAssignment,
    ModuloAssignment,

    Semicolon,

    // Keywords
    ExternKeyword,
    IfKeyword,
    ElseKeyword,
    WhileKeyword,
    AutoKeyword,

    SwitchKeyword,
    CaseKeyword,
    BreakKeyword,

    ArrayIndexing = 52,

    // Alias
    AddressOf = BitwiseAnd,
    PointerDereference = Multiplication,
}

public enum BinaryOperator
{
    None = 0,

    // Arithmetic
    Addition = 15,
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

    // Inline increment and decrement
    Increment,
    Decrement,

    // Bitwise
    BitwiseComplement,
    BitwiseAnd,
    BitwiseOr,
    BitwiseXOr,
    BitwiseShiftLeft,
    BitwiseShiftRight,

    ArrayIndexing = 52,
}

public static class Extensions
{
    public static bool IsEnd(this TokenType t)
    {
        return t is TokenType.Garbage or TokenType.Eof;
    }

    public static bool IsKeyword(this TokenType t)
    {
        return t is TokenType.ExternKeyword or
            TokenType.IfKeyword or
            TokenType.ElseKeyword or
            TokenType.WhileKeyword or
            TokenType.AutoKeyword;
    }

    public static string ToString(this TokenType t)
    {
        return t switch
        {

            TokenType.Semicolon => ";",
            TokenType.Comma => ",",

            TokenType.OpenScope => "{",
            TokenType.CloseScope => "}",

            TokenType.OpenParenthesis => "(",
            TokenType.CloseParenthesis => ")",

            TokenType.OpenBracket => "[",
            TokenType.CloseBracket => "]",

            _ => ((BinaryOperator)t).ToString(),
        };
    }

    public static string ToString(this BinaryOperator t)
    {
        return t switch
        {
            BinaryOperator.None => "None",

            BinaryOperator.Addition => "+",
            BinaryOperator.Subtraction => "-",
            BinaryOperator.Multiplication => "*",
            BinaryOperator.Division => "/",
            BinaryOperator.Modulo => "%",

            BinaryOperator.LessThan => "<",
            BinaryOperator.LessThanEqual => "<=",
            BinaryOperator.GreaterThan => ">",
            BinaryOperator.GreaterThanEqual => ">=",
            BinaryOperator.EqualEqual => "==",
            BinaryOperator.NotEqual => "!=",

            BinaryOperator.BitwiseOr => "|",
            BinaryOperator.LogicalAnd => "&",
            BinaryOperator.LogicalOr => "||",
            BinaryOperator.LogicalNot => "!",
            BinaryOperator.Increment => "++",
            BinaryOperator.Decrement => "--",
            BinaryOperator.BitwiseComplement => throw new System.NotImplementedException(),
            BinaryOperator.BitwiseAnd => throw new System.NotImplementedException(),
            BinaryOperator.BitwiseXOr => throw new System.NotImplementedException(),
            BinaryOperator.BitwiseShiftLeft => throw new System.NotImplementedException(),
            BinaryOperator.BitwiseShiftRight => throw new System.NotImplementedException(),
            BinaryOperator.ArrayIndexing => throw new System.NotImplementedException(),
            _ => throw new ParserException("Unhandled tokentype " + t),
        };
    }

}
