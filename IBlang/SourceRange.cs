namespace IBlang;

using System;

public record SourceRange(int Begin, int End)
{
    public SourceRange() : this(0, int.MaxValue) { }

    public SourceRange Merge(SourceRange range2)
    {
        int mergedBegin = Math.Min(Begin, range2.Begin);
        int mergedEnd = Math.Max(End, range2.End);

        return new SourceRange(mergedBegin, mergedEnd);
    }
}
