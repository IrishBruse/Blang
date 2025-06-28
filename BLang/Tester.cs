namespace BLang;

using System;
using System.IO;
using System.Text;
using BLang.Utility;

using static BLang.Utility.Colors;

public class Tester
{
    static readonly TestOptions opt = (TestOptions)options;
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

        if (opt.UpdateSnapshots)
        {
            UpdateSnapshot(testFile, output, time);
        }
        else
        {
            CompareSnapshot(testFile, output, time);
        }
    }

    const char IconPass = '✓';
    const char IconFail = '×';
    const char IconUpdated = 'u';
    const char IconSame = '~';

    static void UpdateSnapshot(string testFile, CompileOutput output, long start)
    {
        string astFile = Path.ChangeExtension(testFile, ".ast");
        string stdFile = Path.ChangeExtension(testFile, ".out");

        (string astPreviousOutput, string stdPreviousOutput) = LoadTestContent(testFile);

        Executable runOutput = Executable.Capture(output.Executable);

        StringBuilder astOutput = new();
        astOutput.Append(output.AstOutput);
        if (astOutput.Length > 0) File.WriteAllText(astFile, astOutput.ToString());

        StringBuilder stdOutput = new();
        stdOutput.Append(runOutput.StdOut);
        stdOutput.Append(runOutput.StdError);
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

    static void CompareSnapshot(string testFile, CompileOutput output, long start)
    {
        (string astOutput, string stdOutput) = LoadTestContent(testFile);

        Executable runOutput = Executable.Capture(output.Executable);

        string runOutputStdErr = runOutput.StdOut + runOutput.StdError;

        string? error = null;

        bool success = true;
        success &= output.Success;
        if (!success && error == null) error = $"compile failed: {output.Errors}";
        success &= runOutput.ExitCode == 0;
        if (!success && error == null) error = $"exitCode: {runOutput.ExitCode}";
        success &= astOutput == output.AstOutput;
        if (!success && error == null) error = $"ast missmatch: {output.AstOutput}";
        success &= stdOutput == runOutputStdErr;
        if (!success && error == null)
        {
            error = "";
            error += "Expected:\n";
            error += stdOutput;
            error += "\n";
            error += "Recieved:\n";
            error += runOutputStdErr;
        }

        string time = Gray(DateTime.Now.Millisecond - start + "ms");
        string icon = success ? Green(IconPass) : Red(IconFail);

        Log($"{icon} {testFile} {time}");
        if (error != null) Error(error);
    }

    static (string astOutput, string stdOutput) LoadTestContent(string testFile)
    {
        string astFile = Path.ChangeExtension(testFile, ".ast");
        string stdFile = Path.ChangeExtension(testFile, ".out");

        string astOutput = File.Exists(astFile) ? File.ReadAllText(astFile) : "";
        string stdOutput = File.Exists(stdFile) ? File.ReadAllText(stdFile) : "";

        return (astOutput, stdOutput);
    }
}
