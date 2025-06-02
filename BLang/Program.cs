namespace BLang;

using System;
using System.IO;
using System.Text;
using BLang.Utility;

public class Program
{
    public static void Main(string[] args)
    {
        Flags.Parse(args);

        Flags flags = Flags.Instance;

        if (flags.Test)
        {
            flags.Debug = true;
            flags.Run = true;
            Test();
            return;
        }

        CompileOutput output = Compiler.Compile(flags.File);

        if (flags.Debug && !string.IsNullOrEmpty(output.AstOutput))
        {
            if (flags.Debug)
            {
                Console.WriteLine("==========    Ast   ==========");
            }
            Console.WriteLine(output.AstOutput);
        }

        if (!string.IsNullOrEmpty(output.Errors))
        {
            if (flags.Debug)
            {
                Console.WriteLine("==========  Errors  ==========");
            }
            Terminal.Error(output.Errors);
        }

        if (!string.IsNullOrEmpty(output.RunOutput))
        {
            if (flags.Debug)
            {
                Console.WriteLine("==========  Output  ==========");
            }
            Console.WriteLine(output.RunOutput);
        }
    }

    public static void Test()
    {
        string[] files = Directory.GetFiles("../Tests/", "*.b");

        foreach (string testFile in files)
        {
            CompileOutput output = Compiler.Compile(testFile);

            string testOutputFile = Path.ChangeExtension(testFile, ".test");
            string previousTestOutput = File.Exists(testOutputFile) ? File.ReadAllText(testOutputFile) : "";

            if (Flags.Instance.UpdateSnapshots)
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

        string? runOutput = output.Success ? Executable.Run(output.Executable) : output.Errors;
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
            Console.Write("\x1B[34m");
        }

        Console.WriteLine($"Test {status}: {testFile}");
        Console.Write("\x1b[0m");
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

        Terminal.Error(output.Errors);
    }
}
