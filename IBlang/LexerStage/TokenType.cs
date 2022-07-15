namespace IBlang.LexerStage;

public enum TokenType
{
    Garbage = -1,

    Identifier,

    NumberLiteral,
    StringLiteral,
    CharLiteral,

    Keyword,
    Operator,

    Bracket,
    OpenParenthesis,
    CloseParenthesis,
    OpenBracket,
    CloseBracket,
    OpenScope,
    CloseScope,

    LessThan,
    GreaterThan,
    And,
    Pipe,

    Dot,
    Comma,
    Tilda,
    Semicolon,
    Colon,
    Question,

    Plus,
    Minus,
    Multiply,
    Divide,
    Module,

    Caret,
    Exclemation,
    Dollar,

    Equal,

    Eol,
    Eof,
}
