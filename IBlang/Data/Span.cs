namespace IBlang.Data;

public record Span(string File, int Start, int End)
{
    public static readonly Span Empty = new("", 1, 0);

    public override string ToString()
    {
        return $"{File}:{Start}:{End}";
    }
}
