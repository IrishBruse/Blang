namespace BLang;

using System;
using System.IO;
using System.Text;

using BLang.Utility;

using static BLang.Utility.Colors;

public class Tester
{
    private static readonly TestOptions Opt = (TestOptions)Options;
    public static void Test()
    {
        string[] files = Directory.GetFiles("Tests/", "*.b");

        foreach (string testFile in files)
        {
            RunTestFile(testFile);
        }
    }

    private static void RunTestFile(string testFile)
    {
        long time = 0;

        CompileOutput output = new();
        try
        {
            long start = DateTime.Now.Millisecond;
            output = Compiler.Compile(testFile);
            time = Math.Min(DateTime.Now.Millisecond - start, 0);
        }
        catch (Exception e)
        {
            output.Errors = e.ToString();
        }

        try
        {
            if (Opt.UpdateSnapshots)
            {
                UpdateSnapshot(testFile, output, time);
            }
            else
            {
                CompareSnapshot(testFile, output, time);
            }
        }
        catch (Exception e)
        {
            Log($"{Red("E")} {testFile} {Gray(time + "ms")}");
            Error(e.ToString());
        }
    }

    private const char IconPass = '✓';
    private const char IconFail = '×';
    private const char IconUpdated = 'u';
    private const char IconSame = '~';

    private static void UpdateSnapshot(string testFile, CompileOutput output, long start)
    {
        string astFile = Path.ChangeExtension(testFile, ".ast");
        string stdFile = Path.ChangeExtension(testFile, ".out");

        (string astPreviousOutput, string stdPreviousOutput) = LoadTestContent(testFile);

        Executable runOutput = Executable.Capture(output.Executable);

        StringBuilder astOutput = new();
        _ = astOutput.Append(output.AstOutput);
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

        string time = Gray(DateTime.Now.Millisecond - start + "ms");
        Log($"{testIcon} {testFile} {time}");
        if (anyChanges)
        {
            Log($"  {astIcon} {astFile}");
            Log($"  {stdIcon} {stdFile}");
        }
    }

    private static void CompareSnapshot(string testFile, CompileOutput output, long start)
    {
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
            error = $"ast missmatch: {output.AstOutput}";
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

        string time = Gray(DateTime.Now.Millisecond - start + "ms");
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
}
