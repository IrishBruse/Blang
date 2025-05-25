namespace IBlang.Targets;

using System;

public class BaseTarget
{
    public TargetWriter output { private get; set; } = null!;
    int depth { get; set; }

    public void Indent() => depth++;
    public void Dedent() => depth--;

    public void WriteIndentation()
    {
        output.Write(new string(' ', depth * 4));
    }

    public void WriteIndented(string? value)
    {
        output.WriteLine(new string(' ', depth * 4) + value);
    }

    public void Write(string? value)
    {
        output.Write(value);
    }

    public void WriteLine(string? value = "")
    {
        output.WriteLine(value);
    }

    public static void TODO(string message = "")
    {
        string stackTrace = string.Join('\n', Environment.StackTrace.Split('\n')[2..]);

        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"TODO: {message}\n" + stackTrace);
        Console.ResetColor();

        Environment.Exit(-1);
    }
}
