namespace IBlang;

public class Token
{
    public string Value { get; }
    public TokenType Type { get; }
    public Span Span { get; }

    public Token(string value, TokenType type, Span span)
    {
        Value = value;
        Type = type;
        Span = span;
    }

    public override string ToString()
    {
        return $"{Span} {Type} '{Value}'";
    }
}
