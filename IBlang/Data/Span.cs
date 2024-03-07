namespace IBlang.Data;

public record Span(string File, int Start, int End)
{
    public static readonly Span Zero = new("", 0, 0);

    public override string ToString()
    {
        return $"{File}:{Start}:{End}";
    }
}
