namespace BLang.Targets;

using System.Text;

public class BaseTarget
{
    public virtual int Indention { get; } = 2;

    public StringBuilder Output { internal get; set; } = new();
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
        _ = Output.Append(value);
    }

    public void Write(string? value = "")
    {
        _ = string.IsNullOrEmpty(value) ? Output.AppendLine() : Output.AppendLine(Space + value);
    }
}
