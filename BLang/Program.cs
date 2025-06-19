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

        if (options == null)
        {
            return;
        }

        if (options.Test)
        {
            options.Debug = true;
            options.Run = true;
            Test();
            return;
        }

        CompileOutput output = Compiler.Compile(options.File);

        if (options.Debug && !string.IsNullOrEmpty(output.AstOutput))
        {
            Console.WriteLine("===========  Ast  ============");
            Log(output.AstOutput, null);
        }

        if (!string.IsNullOrEmpty(output.Errors))
        {
            Console.WriteLine("==========  Errors  ==========");
            Error(output.Errors, null);
        }

        if (!string.IsNullOrEmpty(output.RunOutput))
        {
            Console.WriteLine("==========  Output  ==========");
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

        bool success = output.Success && astOutput == output.AstOutput && runOutput.Trim() == output.RunOutput?.Trim();

        string status = success ? "Passed" : "Failed";
        Console.Write(success ? "\x1B[32m" : "\x1B[31m");
        Console.WriteLine($"Test {status}: {testFile}");
        Console.Write("\x1b[0m");

        Error(output.Errors);
    }
}
