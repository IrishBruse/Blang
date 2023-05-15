namespace IBlang;

public class Token
{
    public string Value { get; }
    public TokenType Type { get; }
    public int Start { get; }
    public int End { get; }

    public Token(string value, TokenType type, int start, int end)
    {
        Value = value;
        Type = type;
        Start = start;
        End = end;
    }
}
