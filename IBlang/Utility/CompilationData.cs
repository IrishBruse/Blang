namespace IBlang.Utility;

using System.Collections.Generic;

public class CompilationData
{
    public readonly List<int> Lines = [0];

    public string? File { get; internal set; }

    public (int Line, int Column) GetLineColumnFromIndex(int index)
    {
        if (Lines.Count == 0)
        {
            return (1, index + 1);
        }

        int line = 0;
        int lineOffset = 0;
        for (int i = 0; i < Lines.Count; i++)
        {
            if (index > Lines[i])
            {
                lineOffset = Lines[i];
                line = i;
            }
        }

        int column = index - lineOffset;

        return (line + 1, column + 1);
    }

    public string GetFileLocation(int index)
    {
        (int line, int col) = GetLineColumnFromIndex(index);

        return $"{File}({line},{col})";
    }
}
