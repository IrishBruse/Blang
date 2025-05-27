namespace IBlang.Targets;

using System;
using System.IO;
using IBlang.Utility;

public class BaseTarget
{
    public virtual string Target { get; } = "unknown";

    public TargetWriter output { internal get; set; } = null!;
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

    public (string, string) GetOutputFile(string inputFile)
    {
        CreateOutputDirectories(inputFile);

        string projectDirectory = Path.GetDirectoryName(inputFile)!;
        string sourceFileName = Path.GetFileNameWithoutExtension(inputFile);
        string objFile = Path.Combine(projectDirectory, "obj", Target, sourceFileName);
        string binFile = Path.Combine(projectDirectory, "bin", Target, sourceFileName);

        return (objFile, binFile);
    }

    public void CreateOutputDirectories(string inputFile)
    {
        string projectDirectory = Path.GetDirectoryName(inputFile)!;

        string targetDirectory;

        targetDirectory = Path.Combine(projectDirectory, "obj", Target);
        Directory.CreateDirectory(targetDirectory);

        targetDirectory = Path.Combine(projectDirectory, "bin", Target);
        Directory.CreateDirectory(targetDirectory);
    }
}
