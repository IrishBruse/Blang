namespace BLang.Targets.qbe;

using System.Text;

public partial class QbeOutput
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

    public void Label(string value)
    {
        _ = Text.AppendLine(Space + "@" + value);
    }

    public void Write(string value)
    {
        _ = Text.AppendLine(Space + value);
    }

    public void WriteLine()
    {
        _ = Text.AppendLine();
    }

    public void Comment(params string[] message)
    {
        _ = Text.AppendLine(Space + "# " + string.Join(' ', message));
    }

    public void Clear()
    {
        _ = Text.Clear();
        Depth = 0;
    }
}
