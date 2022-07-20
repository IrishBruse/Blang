namespace IBlang.LexerStage;

public record Token(TokenType Type, string Value, Loc Span)
{
    public override string ToString()
    {
        return $"{Span.Line}:{Span.Column} {Type} \"{Value}\"";
    }
}
