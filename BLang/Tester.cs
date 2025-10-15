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
    private const char IconFail = '×';
    private const char IconUpdated = 'u';
    private const char IconSame = '~';

    public static void TestFiles(string[] tests)
    {
        Stopwatch sw = Stopwatch.StartNew();
        int passed = 0;
        foreach (string testFile in tests)
        {
            if (TestFile(testFile))
            {
                passed++;
            }
        }
        Console.WriteLine();
        Console.WriteLine("Tests finished in " + sw.Elapsed.TotalSeconds.ToString("0.00") + "s");
        Console.WriteLine($"{passed}/{tests.Length} Passed");
        Console.WriteLine();
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
        string folderType = testFile.Split("/")[1];
        (string astPreviousOutput, string stdPreviousOutput) = LoadTestContent(testFile);

        Options.Verbose = 0; // Force readable messages and no traces

        Result<CompileOutput> res = Compiler.Compile(testFile);

        string astFile = Path.ChangeExtension(testFile, ".ast.json");
        string stdFile = Path.ChangeExtension(testFile, ".out");

        if (testFile.StartsWith("Examples/", StringComparison.Ordinal))
        {
            folderType = "example";
        }

        if (folderType == "ok")
        {
            if (!res.IsSuccess)
            {
                Log($"{Red(IconFail)} {testFile}");
                Error(res.Error);
                return;
            }

            CompileOutput output = res.Value;

            Executable runOutput = Executable.Run(output.Executable);

            StringBuilder stdOutput = new();
            _ = stdOutput.Append(runOutput.StdOut);
            _ = stdOutput.Append(runOutput.StdError);
            if (stdOutput.Length > 0) File.WriteAllText(stdFile, stdOutput.ToString());

            string astOutput = output.CompilationUnit.ToJson();
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
            if (res.IsSuccess)
            {
                Log($"{Red(IconFail)} {testFile}");
                Error("Test did not produce a compile error as expected");
                return;
            }

            string error = res.Error;

            if (error.Length > 0) File.WriteAllText(stdFile, error.ToString());

            bool stdChanged = !error.Equals(stdPreviousOutput, StringComparison.Ordinal);
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
            Console.WriteLine("Unknown folderType " + folderType);
        }
    }

    private static bool CompareSnapshot(string testFile)
    {
        string folderType = testFile.Split("/")[1];

        Result<CompileOutput> res = Compiler.Compile(testFile);

        bool passed = false;

        string error = "";

        if (testFile.Contains("Examples/"))
        {
            folderType = "example";
        }

        if (folderType == "ok" || folderType == "example")
        {
            passed = CompareSuccess(testFile, folderType, res, ref error);
        }
        else if (folderType == "error")
        {
            passed = CompareError(testFile, res, ref error);
        }
        else
        {
            Console.WriteLine("Unkown folderType " + folderType);
        }

        long ms = res.IsSuccess ? res.Value.CompileTime : 0;

        string time = Gray($"({ms}ms)");
        string icon = error == string.Empty ? Green(IconPass) : Red(IconFail);
        Log($"{icon} {testFile} {time}");
        if (error != string.Empty) Error(error);

        return passed;
    }

    private static bool CompareSuccess(string testFile, string folderType, Result<CompileOutput> res, ref string error)
    {
        (string astOutput, string stdOutput) = LoadTestContent(testFile);
        if (!res.IsSuccess)
        {
            error = res.Error;
        }
        else
        {
            CompileOutput output = res.Value;

            string astJson = output!.CompilationUnit.ToJson();

            Executable runOutput = Executable.Run(output.Executable);

            if (runOutput.ExitCode != 0)
            {
                error += $"ExitCode: {runOutput.ExitCode}\n";
                error += "\n";
            }

            if (folderType != "example")
            {
                if (!astJson.Equals(astOutput, StringComparison.Ordinal))
                {
                    (int line, string? line1, string? line2) = FindFirstDifferentLine(astOutput, astJson);

                    error += $"""
                {Path.ChangeExtension(testFile, ".ast.json")}:{line}
                Expected Ast:
                {line1}
                Recieved:
                {line2}

                """.Trim();
                    error += "\n";
                }

                if (!stdOutput.Equals(runOutput.StdOut, StringComparison.Ordinal))
                {
                    (int line, string? line1, string? line2) = FindFirstDifferentLine(stdOutput, runOutput.StdOut!);

                    error += $"""
                {Path.ChangeExtension(testFile, ".out")}:{line}
                Expected Output:
                {line1}
                Recieved:
                {line2}

                """.Trim();
                    error += "\n";
                }
            }
        }

        return res.IsSuccess;
    }

    private static bool CompareError(string testFile, Result<CompileOutput> res, ref string error)
    {
        (_, string stdOutput) = LoadTestContent(testFile);
        string compileError = res.Error;

        if (!stdOutput.Equals(compileError, StringComparison.Ordinal))
        {
            error += $"CompileError: {compileError}\n";
        }

        return !res.IsSuccess;
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
