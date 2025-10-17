namespace BLang.Utility;

public record SourceRange(int Start, int End)
{
    public static SourceRange Zero => new(0, 0);

    public SourceRange() : this(0, int.MaxValue) { }

    public override string ToString()
    {
        return $"{Start}-{End}";
    }
}
