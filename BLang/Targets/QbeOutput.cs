namespace BLang.Targets;

using System.Text;

public class QbeOutput
{
    public virtual int Indention { get; } = 4;

    public StringBuilder Text { get; set; } = new();
    private int Depth { get; set; }

    public void Indent(int? spaces = null)
    {
        Depth += spaces ?? Indention;
    }

    public void Dedent(int? spaces = null)
    {
        Depth -= spaces ?? Indention;
    }

    public string Space => new(' ', Depth);

    public void WriteRaw(string? value)
    {
        _ = Text.Append(value);
    }

    public void Write(string? value = "")
    {
        _ = string.IsNullOrEmpty(value) ? Text.AppendLine() : Text.AppendLine(Space + value);
    }

    public void Clear()
    {
        _ = Text.Clear();
        Depth = 0;
    }
}
