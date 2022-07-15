namespace IBlang;

public struct Span
{
    public int Start;
    public int End;

    public Span(int start, int end)
    {
        Start = start;
        End = end;
    }

    public override string ToString()
    {
        return $"{Start}-{End}";
    }
}
