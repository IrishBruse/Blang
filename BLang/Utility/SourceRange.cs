namespace BLang.Utility;

using System;

public record SourceRange(int Start, int End)
{
    public static SourceRange Zero => new(0, 0);

    public SourceRange() : this(0, int.MaxValue) { }


    public SourceRange Merge(SourceRange range2)
    {
        int mergedBegin = Math.Min(Start, range2.Start);
        int mergedEnd = Math.Max(End, range2.End);

        return new SourceRange(mergedBegin, mergedEnd);
    }
}
