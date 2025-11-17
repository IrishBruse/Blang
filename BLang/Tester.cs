namespace BLang;

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

using BLang.Utility;

using static BLang.Utility.Colors;

public class Tester
{
    private const char IconPass = '✓';
    private const char IconFail = '\u0078';
    private const char IconUpdated = 'u';
    private const char IconSame = '~';

    public static void TestFiles(string[] tests)
    {
        List<string> failed = new();

        int verboseLevel = Options.Verbose;

        Options.Verbose = 0;

        Stopwatch sw = Stopwatch.StartNew();
        int passed = 0;
        foreach (string testFile in tests)
        {
            if (TestFile(testFile))
            {
                passed++;
            }
            else
            {
                failed.Add(testFile);
            }
        }

        Options.Verbose = Math.Max(1, verboseLevel);

        if (failed.Count > 0 && !Options.UpdateSnapshots)
        {
            Log("");
            Log($"{tests.Length - passed} Tests failed:");
            Log("");
            foreach (string item in failed)
            {
                _ = TestFile(item);
                Log("");
            }
        }

        Log("");
        Log($"Tests finished in {sw.Elapsed.TotalSeconds:0.00}s");
        Log($"{passed}/{tests.Length} Passed");
        Log("");
    }

    public static bool TestFile(string testFile)
    {
        if (Options.UpdateSnapshots)
        {
            Options.Ast = true;
            UpdateSnapshot(testFile);
            return true;
        }
        else
        {
            return CompareSnapshot(testFile);
        }
    }

    private static void UpdateSnapshot(string testFile)
    {
        testFile = Path.GetRelativePath(Environment.CurrentDirectory, testFile);

        string folderType = testFile.Split("/")[1];
        (string astPreviousOutput, string stdPreviousOutput) = LoadTestContent(testFile);

        CompileOutput output = Compiler.Compile(testFile);

        string astFile = Path.ChangeExtension(testFile, ".ast.json");
        string stdFile = Path.ChangeExtension(testFile, ".out");

        if (testFile.StartsWith("Examples/", StringComparison.Ordinal))
        {
            folderType = "example";
        }

        if (folderType == "ok")
        {
            if (!output.Success)
            {
                Log($"{Red(IconFail)} {testFile}");
                Error(output.Error);
                return;
            }

            StringBuilder stdOutput = new();
            if (output.Executable != null)
            {
                Executable runOutput = Executable.Run(output.Executable);
                _ = stdOutput.Append(runOutput.StdOut);
                _ = stdOutput.Append(runOutput.StdError);
            }

            if (stdOutput.Length > 0) File.WriteAllText(stdFile, stdOutput.ToString());

            string astOutput = output.CompilationUnit!.ToJson();
            if (astOutput.Length > 0) File.WriteAllText(astFile, astOutput.ToString());

            bool astChanged = !astOutput.Equals(astPreviousOutput, StringComparison.Ordinal);
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
        else if (folderType == "error")
        {
            if (output.Success)
            {
                Log($"{Red(IconFail)} {testFile}");
                Error("Test did not produce a compile error as expected");
                return;
            }

            string? error = output.Error;

            if (error != null && error.Length > 0) File.WriteAllText(stdFile, error.ToString());

            bool stdChanged = error != null && !error.Equals(stdPreviousOutput, StringComparison.Ordinal);
            string stdIcon = stdChanged ? Green(IconUpdated) : Gray(IconSame);

            bool anyChanges = stdChanged;
            string testIcon = anyChanges ? Green(IconUpdated) : Gray(IconSame);
            Log($"{testIcon} {testFile}");
            if (anyChanges)
            {
                Log($"  {stdIcon} {stdFile}");
            }
        }
        else if (folderType == "example")
        {
            Log($"{Gray(IconSame)} {testFile}");
        }
        else
        {
            Log("Unknown folderType " + folderType);
        }
    }

    private static bool CompareSnapshot(string testFile)
    {
        testFile = Path.GetRelativePath(Environment.CurrentDirectory, testFile);

        string folderType = testFile.Split("/")[1];
        string? error = null;

        if (testFile.Contains("Examples/"))
        {
            folderType = "example";
        }

        Stopwatch timer = Stopwatch.StartNew();

        CompileOutput output = Compiler.Compile(testFile);

        bool passed = false;
        if (folderType == "ok" || folderType == "example")
        {
            passed = CompareSuccess(testFile, folderType, output, ref error);
        }
        else if (folderType == "error")
        {
            passed = CompareError(testFile, output, ref error);
        }
        else
        {
            Log("Unkown folderType " + folderType);
        }

        double ms = timer.ElapsedTicks / 1000000.0;

        string time = Gray($"({ms:0.00}ms)");
        string icon = error == null ? Green(IconPass) : Red(IconFail);
        Log($"{icon} {testFile} {time}");
        if (error != string.Empty) Error(error);

        return passed;
    }

    private static bool CompareSuccess(string testFile, string folderType, CompileOutput output, ref string? error)
    {
        (string astOutput, string stdOutput) = LoadTestContent(testFile);
        bool passed = output.Success;
        if (!output.Success)
        {
            error = output.Error;
        }
        else
        {
            string astJson = output.CompilationUnit!.ToJson();

            Executable runOutput = Executable.Run(output.Executable!);

            if (runOutput.ExitCode != 0)
            {
                error += $"ExitCode: {runOutput.ExitCode}\n";
                error += "\n";
                passed = false;
            }

            if (folderType != "example")
            {
                if (!astJson.Equals(astOutput, StringComparison.Ordinal))
                {
                    (int line, string? line1, string? line2) = FindFirstDifferentLine(astOutput, astJson);

                    error += $"""
                {Path.ChangeExtension(testFile, ".ast.json")}:{line}

                Expected:
                {line1}
                Recieved:
                {line2}

                """.Trim();
                    error += "\n";
                    passed = false;
                }

                if (!stdOutput.Equals(runOutput.StdOut, StringComparison.Ordinal))
                {
                    (int line, string? line1, string? line2) = FindFirstDifferentLine(stdOutput, runOutput.StdOut!);

                    error += $"""
                {Path.ChangeExtension(testFile, ".out")}:{line}
                Expected:
                {line1}
                Recieved:
                {line2}

                """.Trim();
                    error += "\n";
                    passed = false;
                }
            }
        }

        return passed;
    }

    private static bool CompareError(string testFile, CompileOutput res, ref string? error)
    {
        (_, string stdOutput) = LoadTestContent(testFile);
        string? compileError = res.Error;

        if (!stdOutput.Equals(compileError, StringComparison.Ordinal))
        {
            error += $"CompileError: {compileError}\n";
        }

        return !res.Success;
    }

    private static (string astOutput, string stdOutput) LoadTestContent(string testFile)
    {
        string astFile = Path.ChangeExtension(testFile, ".ast.json");
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

        if (reader1.ReadLine() != null || reader2.ReadLine() != null)
        {
            if (reader1.Peek() != -1)
            {
                line1 = reader2.ReadLine();
                line2 = reader2.ReadLine();
                return Tuple.Create(lineNumber, line1, line2);
            }
            else
            {
                return Tuple.Create<int, string?, string?>(lineNumber, string.Empty, string.Empty);
            }
        }

        return Tuple.Create<int, string?, string?>(0, string.Empty, string.Empty);
    }
}
