namespace BLang;

using System;
using System.IO;
using System.Text;
using BLang.Utility;

public class Program
{
    public static void Main(string[] args)
    {
        bool watching = Environment.GetEnvironmentVariable("DOTNET_WATCH") == "1";

        if (watching)
        {
            Console.Write(new string('\n', 10));
        }

        Options.Parse(args);

        try
        {
            Execute();
        }
        catch (Exception e)
        {
            Error(e.ToString());
        }
    }

    public static void Execute()
    {
        if (options.Verb == Verb.Test)
        {
            Test();
            return;
        }

        CompileOutput output = Compiler.Compile(options.File, options.Verb == Verb.Run);

        if (!string.IsNullOrEmpty(output.Errors))
        {
            Debug("==========  Errors  ==========");
            Error(output.Errors, null);
        }

        if (!string.IsNullOrEmpty(output.RunOutput))
        {
            Debug("==========  Output  ==========");
            Log(output.RunOutput);
        }
    }

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
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"Running Test {testFile}");

        CompileOutput output = new();
        try
        {
            output = Compiler.Compile(testFile, true);
        }
        catch (Exception e)
        {
            output.Errors = e.ToString();
        }

        string testOutputFile = Path.ChangeExtension(testFile, ".test");
        string previousTestOutput = File.Exists(testOutputFile) ? File.ReadAllText(testOutputFile) : "";

        if (options.UpdateSnapshots)
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
        string runOutput = parts.Length > 1 ? parts[1].Trim() : string.Empty;

        Console.CursorTop--;
        bool success = output.Success && string.IsNullOrEmpty(output.Errors) && astOutput == output.AstOutput && runOutput.Trim() == output.RunOutput?.Trim();
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
