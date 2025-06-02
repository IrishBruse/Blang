namespace BLang.Targets;

using System;
using System.IO;
using System.Text;
using BLang.AstParser;
using BLang.Utility;

public class BaseTarget
{
    public virtual string Target { get; } = "unknown";

    public StringBuilder output { internal get; set; } = new();
    int depth { get; set; }

    public void Indent() => depth++;
    public void Dedent() => depth--;

    public void WriteIndentation()
    {
        output.Append(new string(' ', depth * 4));
    }

    public void WriteIndented(string? value)
    {
        output.AppendLine(new string(' ', depth * 4) + value);
    }

    public void Write(string? value)
    {
        output.Append(value);
    }

    public void WriteLine(string? value = "")
    {
        output.AppendLine(value);
    }
}
