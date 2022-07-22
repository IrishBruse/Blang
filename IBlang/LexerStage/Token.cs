namespace IBlang.LexerStage;

public record Token(TokenType Type, string Value, Span Span)
{
    public override string ToString()
    {
        return $"{Type} \"{Value}\"";
    }
}
