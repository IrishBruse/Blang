namespace IBlang;

using System;
using System.IO;
using System.Text;
using IBlang.Utility;

public class Program
{
    public static void Main(string[] args)
    {
        Globals.ParseArgs(args, out string? file);

        if (Flags.Test)
        {
            Flags.Ast = true;
            Flags.Run = true;
            Test();
            return;
        }

        CompileOutput output = Compiler.Compile(file);

        Console.Write(output.RunOutput);
    }

    public static void Test()
    {
        string[] files = Directory.GetFiles("../Tests/", "*.b");

        foreach (string testFile in files)
        {
            CompileOutput output = Compiler.Compile(testFile);

            string testOutputFile = Path.ChangeExtension(testFile, ".test");
            string previousTestOutput = File.Exists(testOutputFile) ? File.ReadAllText(testOutputFile) : "";

            if (Flags.UpdateSnapshots)
            {
                UpdateSnapshot(testFile, testOutputFile, output, previousTestOutput);
            }
            else
            {
                CompareSnapshot(testFile, output, previousTestOutput);
            }

        }
    }

    static void CompareSnapshot(string testFile, CompileOutput output, string previousTestOutput)
    {
        string[] parts = previousTestOutput.Split("==============================\n");

        string astOutput = parts[0].Trim();
        string runOutput = parts.Length > 1 ? parts[1].Trim() : string.Empty;

        bool success = astOutput == output.AstOutput && runOutput.Trim() == output.RunOutput.Trim();

        string status = success ? "Passed" : "Failed";
        Console.Write(success ? "\x1B[32m" : "\x1B[31m");
        Console.WriteLine($"Test {status}: {testFile}");
        Console.Write("\x1b[0m");
    }

    static void UpdateSnapshot(string testFile, string testOutputFile, CompileOutput output, string previousTestOutput)
    {
        StringBuilder testOutput = new();
        testOutput.AppendLine(output.AstOutput);

        if (output.Success)
        {
            string runOutput = Compiler.RunExecutable(output.Executable);
            if (!string.IsNullOrEmpty(runOutput))
            {
                testOutput.AppendLine("==============================");
                testOutput.Append(runOutput);
            }
        }
        else
        {
            string runOutput = output.Errors;
            if (!string.IsNullOrEmpty(runOutput))
            {
                testOutput.AppendLine("==============================");
                testOutput.Append(runOutput);
            }
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
}
