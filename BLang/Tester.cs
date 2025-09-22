namespace BLang;

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

using BLang.Utility;

using static BLang.Utility.Colors;

public class Tester
{
    public static void Test(string path)
    {
        string[] tests = Directory.GetFiles(path, "*.b", SearchOption.AllDirectories);

        Options.Ast = true;

        foreach (string testFile in tests)
        {
            RunTestFile(testFile);
        }
    }

    public static void RunTestFile(string testFile)
    {
        if (Options.UpdateSnapshots)
        {
            UpdateSnapshot(testFile);
        }
        else
        {
            CompareSnapshot(testFile);
        }
    }

    private const char IconPass = '✓';
    private const char IconFail = '×';
    private const char IconUpdated = 'u';
    private const char IconSame = '~';

    private static void UpdateSnapshot(string testFile)
    {
        Options.Ast = true;

        if (!Compiler.TryCompile(testFile, out CompileOutput? output))
        {
            return;
        }

        string astFile = Path.ChangeExtension(testFile, ".ast");
        string stdFile = Path.ChangeExtension(testFile, ".out");

        (string astPreviousOutput, string stdPreviousOutput) = LoadTestContent(testFile);

        if (!output.Success) return;

        Executable runOutput = Executable.Capture(output.Executable);

        StringBuilder astOutput = new();
        _ = astOutput.Append("output.AstOutput");
        if (astOutput.Length > 0) File.WriteAllText(astFile, astOutput.ToString());

        StringBuilder stdOutput = new();
        _ = stdOutput.Append(runOutput.StdOut);
        _ = stdOutput.Append(runOutput.StdError);
        if (stdOutput.Length > 0) File.WriteAllText(stdFile, stdOutput.ToString());

        bool astChanged = !astOutput.Equals(astPreviousOutput);
        string astIcon = astChanged ? Green(IconUpdated) : Gray(IconSame);

        bool stdChanged = !stdOutput.Equals(stdPreviousOutput);
        string stdIcon = stdChanged ? Green(IconUpdated) : Gray(IconSame);

        bool anyChanges = astChanged || stdChanged;
        string testIcon = anyChanges ? Green(IconUpdated) : Gray(IconSame);
        Log($"{testIcon} {testFile}");
        if (anyChanges)
        {
            Log($"  {astIcon} {astFile}");
            Log($"  {stdIcon} {stdFile}");
        }
    }

    private static void CompareSnapshot(string testFile)
    {
        Stopwatch sw = Stopwatch.StartNew();
        if (!Compiler.TryCompile(testFile, out CompileOutput? output))
        {
            return;
        }
        long ms = sw.ElapsedMilliseconds;

        (string astOutput, string stdOutput) = LoadTestContent(testFile);

        Executable runOutput = Executable.Capture(output.Executable);

        string runOutputStdErr = runOutput.StdOut + runOutput.StdError;

        string error = "";

        bool success = false;

        if (!output.Success || !string.IsNullOrEmpty(output.Errors))
        {
            error = $"compile failed: {output.Errors}";
        }
        else if (runOutput.ExitCode != 0)
        {
            error = $"exitCode: {runOutput.ExitCode}";
        }
        else if (astOutput != output.AstOutput)
        {
            (int line, string? line1, string? line2) = FindFirstDifferentLine(astOutput, output.AstOutput);
            // error = $"Mismatch at {Path.ChangeExtension(testFile, ".ast")}:{line}\n{text}";
            error = $"""
            Mismatch at {Path.ChangeExtension(testFile, ".ast")}:{line}
            Expected:
            {line1}
            Recieved:
            {line2}
            """;
        }
        else if (stdOutput != runOutputStdErr)
        {
            error = $"""
            Expected:
            {stdOutput}
            Recieved:
            {runOutputStdErr}
            """;
        }
        else
        {
            success = true;
        }
        string time = Gray($"({ms}ms)");
        string icon = success ? Green(IconPass) : Red(IconFail);

        Log($"{icon} {testFile} {time}");
        if (error != null) Error(error);
    }

    private static (string astOutput, string stdOutput) LoadTestContent(string testFile)
    {
        string astFile = Path.ChangeExtension(testFile, ".ast");
        string stdFile = Path.ChangeExtension(testFile, ".out");

        string astOutput = File.Exists(astFile) ? File.ReadAllText(astFile) : "";
        string stdOutput = File.Exists(stdFile) ? File.ReadAllText(stdFile) : "";

        return (astOutput, stdOutput);
    }

    public static Tuple<int, string?, string?> FindFirstDifferentLine(string content1, string content2)
    {
        using StringReader reader1 = new(content1);
        using StringReader reader2 = new(content2);
        string? line1;
        string? line2;
        int lineNumber = 1;

        while ((line1 = reader1.ReadLine()) != null && (line2 = reader2.ReadLine()) != null)
        {
            if (line1 != line2)
            {
                return Tuple.Create<int, string?, string?>(lineNumber, line1, line2);
            }
            lineNumber++;
        }

        // Check for differences in length
        if (reader1.ReadLine() != null || reader2.ReadLine() != null)
        {
            // If content1 is longer, we return the first line of content1 that doesn't have a match in content2.
            if (reader1.Peek() != -1)
            {
                line1 = reader2.ReadLine();
                line2 = reader2.ReadLine();
                return Tuple.Create(lineNumber, line1, line2);
            }
            // If content2 is longer, the first different line is the end of content1.
            // We return null for the line content since content1 has no more lines.
            else
            {
                return Tuple.Create<int, string?, string?>(lineNumber, string.Empty, string.Empty);
            }
        }

        // If we reach here, strings are identical
        return Tuple.Create<int, string?, string?>(0, string.Empty, string.Empty);
    }
}
