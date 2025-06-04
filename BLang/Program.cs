namespace BLang;

using System;
using System.IO;
using System.Text;
using BLang.Utility;

public class Program
{
    public static void Main(string[] args)
    {
        Options? options = Options.Parse(args);

        if (options == null)
        {
            return;
        }

        if (options.Test)
        {
            options.Debug = true;
            options.Run = true;
            Test(options);
            return;
        }

        Compiler compiler = new(options);

        CompileOutput output = compiler.Compile(options.File);

        if (options.Debug && !string.IsNullOrEmpty(output.AstOutput))
        {
            if (options.Debug)
            {
                Console.WriteLine("=========== Ast ============");
            }
            Log(output.AstOutput, null);
        }

        if (!string.IsNullOrEmpty(output.Errors))
        {
            if (options.Debug)
            {
                Console.WriteLine("==========  Errors  ==========");
            }
            Error(output.Errors, null);
        }

        if (!string.IsNullOrEmpty(output.RunOutput))
        {
            if (options.Debug)
            {
                Console.WriteLine("==========  Output  ==========");
            }
            Console.WriteLine(output.RunOutput);
        }
    }

    public static void Test(Options options)
    {
        string[] files = Directory.GetFiles("../Tests/", "*.b");

        Compiler compiler = new(options);

        foreach (string testFile in files)
        {
            CompileOutput output = compiler.Compile(testFile);

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
    }

    static void UpdateSnapshot(string testFile, string testOutputFile, CompileOutput output, string previousTestOutput)
    {
        StringBuilder testOutput = new();
        testOutput.AppendLine(output.AstOutput);

        string? runOutput = Executable.Run(output.Executable).Match(sucess => sucess, error => error.Value);
        if (!string.IsNullOrEmpty(runOutput))
        {
            testOutput.AppendLine("==============================");
            testOutput.Append(runOutput);
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

        bool success = output.Success && astOutput == output.AstOutput && runOutput.Trim() == output.RunOutput?.Trim();

        string status = success ? "Passed" : "Failed";
        Console.Write(success ? "\x1B[32m" : "\x1B[31m");
        Console.WriteLine($"Test {status}: {testFile}");
        Console.Write("\x1b[0m");

        Error(output.Errors);
    }
}
