namespace BLang.Targets;

using System;
using System.Text;

public class BaseTarget
{
    public virtual int Indention { get; } = 2;

    public StringBuilder output { internal get; set; } = new();
    int depth { get; set; }

    public void Indent() => depth++;
    public void Dedent() => depth--;

    public string Space => new(' ', depth * Indention);

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
