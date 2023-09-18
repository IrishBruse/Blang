namespace IBlang.Data;

public record Span(string File, int Start, int End)
{
    public override string ToString()
    {
        return $"{File}:{Start}:{End}";
    }
}
