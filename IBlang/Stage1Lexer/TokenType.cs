namespace IBlang.Stage1Lexer;

public enum TokenType
{
    // Misc
    Eol = 254,
    Eof = 256,
    Garbage = -1,

    // Keywords
    KeywordFunc,
    KeywordIf,
    KeywordElse,
    KeywordReturn,

    Identifier,

    IntegerLiteral,
    FloatLiteral,
    StringLiteral,
    CharLiteral,

    // Bracket Pairs
    OpenParenthesis,
    CloseParenthesis,
    OpenBracket,
    CloseBracket,
    OpenScope,
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
    TypeInteger,
    TypeFloat,
    TypeString,
    TypeSigned8BitInteger,
    TypeSigned16BitInteger,
    TypeSigned32BitInteger,
    TypeSigned64BitInteger,
    TypeUnsigned8BitInteger,
    TypeUnsigned16BitInteger,
    TypeUnsigned32BitInteger,
    TypeUnsigned64BitInteger,
}
