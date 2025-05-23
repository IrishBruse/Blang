namespace IBlang;

using System;

public record Range(string file, int Begin, int End, int Line, int Column)
{
    public Range() : this("", 0, int.MaxValue, 1, 1) { }

    public override string ToString() => $"{file}({Line},{Column}):";

    public Range Merge(Range range2)
    {
        if (file != range2.file)
        {
            throw new ArgumentException("Ranges must be in the same file to be merged.");
        }

        int mergedBegin = Math.Min(Begin, range2.Begin);
        int mergedEnd = Math.Max(End, range2.End);

        return new Range(file, mergedBegin, mergedEnd, Line, Column);
    }
}
