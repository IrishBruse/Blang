namespace BLang.Utility;

using System.Collections.Generic;

public class CompilationData(string file)
{
    public SymbolTable Symbols { get; } = new();
    public List<int> Lines { get; } = [];

    public string File { get; set; } = file;

    public (int Line, int Column) GetLineColumnFromIndex(int index)
    {
        int line = 0;
        int lineOffset = 0;
        for (int i = 0; i < Lines.Count; i++)
        {
            if (index > Lines[i])
            {
                lineOffset = Lines[i] - 1;
                line = i + 1;
            }
        }

        int column = index - lineOffset;

        return (line + 1, column);
    }

    public string GetFileLocation(int index)
    {
        (int line, int col) = GetLineColumnFromIndex(index);

        return $"{File}:{line}:{col}";
    }
}
