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

            TokenType.BitwiseOr => "|",
            TokenType.Eof => throw new System.NotImplementedException(),
            TokenType.Garbage => throw new System.NotImplementedException(),
            TokenType.None => throw new System.NotImplementedException(),
            TokenType.Comment => throw new System.NotImplementedException(),
            TokenType.Identifier => throw new System.NotImplementedException(),
            TokenType.IntegerLiteral => throw new System.NotImplementedException(),
            TokenType.FloatLiteral => throw new System.NotImplementedException(),
            TokenType.StringLiteral => throw new System.NotImplementedException(),
            TokenType.CharLiteral => throw new System.NotImplementedException(),
            TokenType.Dot => throw new System.NotImplementedException(),
            TokenType.LogicalAnd => throw new System.NotImplementedException(),
            TokenType.LogicalOr => throw new System.NotImplementedException(),
            TokenType.LogicalNot => throw new System.NotImplementedException(),
            TokenType.Assignment => throw new System.NotImplementedException(),
            TokenType.AdditionAssignment => throw new System.NotImplementedException(),
            TokenType.SubtractionAssignment => throw new System.NotImplementedException(),
            TokenType.MultiplicationAssignment => throw new System.NotImplementedException(),
            TokenType.DivisionAssignment => throw new System.NotImplementedException(),
            TokenType.ModuloAssignment => throw new System.NotImplementedException(),
            TokenType.Increment => throw new System.NotImplementedException(),
            TokenType.Decrement => throw new System.NotImplementedException(),
            TokenType.BitwiseComplement => throw new System.NotImplementedException(),
            TokenType.BitwiseAnd => throw new System.NotImplementedException(),
            TokenType.BitwiseXOr => throw new System.NotImplementedException(),
            TokenType.BitwiseShiftLeft => throw new System.NotImplementedException(),
            TokenType.BitwiseShiftRight => throw new System.NotImplementedException(),
            TokenType.ExternKeyword => throw new System.NotImplementedException(),
            TokenType.IfKeyword => throw new System.NotImplementedException(),
            TokenType.ElseKeyword => throw new System.NotImplementedException(),
            TokenType.WhileKeyword => throw new System.NotImplementedException(),
            TokenType.AutoKeyword => throw new System.NotImplementedException(),
            TokenType.SwitchKeyword => throw new System.NotImplementedException(),
            TokenType.CaseKeyword => throw new System.NotImplementedException(),
            TokenType.BreakKeyword => throw new System.NotImplementedException(),
            TokenType.ArrayIndexing => throw new System.NotImplementedException(),
            _ => throw new ParserException("Unhandled tokentype " + t),
        };
    }

}
