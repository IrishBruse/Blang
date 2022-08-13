namespace IBlang.Stage1Lexer;

public record Token(TokenType Type, string Value, Span Span)
{
    public override string ToString() => $"{Type} \"{Value}\"";
}
