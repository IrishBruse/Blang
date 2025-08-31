namespace BLang.Targets;

using System.Text;

public class BaseTarget
{
    public virtual int Indention { get; } = 2;

    public StringBuilder output { internal get; set; } = new();
    int depth { get; set; }

    public void Indent(int? spaces = null) => depth += spaces ?? Indention;
    public void Dedent(int? spaces = null) => depth -= spaces ?? Indention;

    public string Space => new(' ', depth);

    public void WriteRaw(string? value)
    {
        output.Append(value);
    }

    public void Write(string? value = "")
    {
        if (string.IsNullOrEmpty(value))
        {
            output.AppendLine();
        }
        else
        {
            output.AppendLine(Space + value);
        }
    }
}
