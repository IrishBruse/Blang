namespace BLang;

using System;
using System.IO;
using System.Text;
using BLang.Utility;

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
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"Running {testFile}");
        Console.ResetColor();

        CompileOutput output = new();
        try
        {
            output = Compiler.Compile(testFile);
        }
        catch (Exception e)
        {
            output.Errors = e.ToString();
        }

        string testOutputFile = Path.ChangeExtension(testFile, ".test");
        string previousTestOutput = File.Exists(testOutputFile) ? File.ReadAllText(testOutputFile) : "";

        if (opt.UpdateSnapshots)
        {
            UpdateSnapshot(testFile, testOutputFile, output, previousTestOutput);
        }
        else
        {
            CompareSnapshot(testFile, output, previousTestOutput);
        }
    }

    static void UpdateSnapshot(string testFile, string testOutputFile, CompileOutput output, string previousTestOutput)
    {
        StringBuilder testOutput = new();
        testOutput.AppendLine(output.AstOutput);

        Executable runOutput = Executable.Capture(output.Executable);
        testOutput.AppendLine("==============================");

        if (!string.IsNullOrEmpty(runOutput.StdOut))
        {
            testOutput.Append(runOutput.StdOut);
        }

        if (!string.IsNullOrEmpty(runOutput.StdError))
        {
            testOutput.Append(runOutput.StdError);
        }

        string newTestOutput = testOutput.ToString();
        File.WriteAllText(testOutputFile, newTestOutput);

        string status = previousTestOutput != newTestOutput ? "Updated" : "Skipped";
        if (previousTestOutput != newTestOutput)
        {
            Error($"Test {status}: {testFile}");
        }
        else
        {
            Console.WriteLine($"Test {status}: {testFile}");
        }
    }

    static void CompareSnapshot(string testFile, CompileOutput output, string previousTestOutput)
    {
        string[] parts = previousTestOutput.Split("==============================\n");

        string astOutput = parts[0].Trim();

        bool success = output.Success && string.IsNullOrEmpty(output.Errors) && astOutput == output.AstOutput;
        if (success)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Test Success: {testFile}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Test Failed: {testFile}");
        }
        Console.ResetColor();

        if (!string.IsNullOrEmpty(output.Errors))
        {
            Error(output.Errors);
            Console.WriteLine();
        }
    }
}
