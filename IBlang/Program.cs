namespace IBlang;

using System;
using System.IO;
using System.Text;
using IBlang.Utility;

public class Program
{
    public static void Main(string[] args)
    {
        Console.Clear();

        Globals.ParseArgs(args, out string? file);
        HandleFlags();

        if (Flags.Test)
        {
            Flags.Ast = true;
            Flags.Run = true;
            Test();
            return;
        }

        CompileOutput output = Compiler.Compile(file);

        if (Flags.Debug && !string.IsNullOrEmpty(output.AstOutput))
        {
            if (Flags.Debug)
            {
                Console.WriteLine("==========    Ast   ==========");
            }
            Console.WriteLine(output.AstOutput);
        }

        if (!string.IsNullOrEmpty(output.Errors))
        {
            if (Flags.Debug)
            {
                Console.WriteLine("==========  Errors  ==========");
            }
            Terminal.Error(output.Errors);
        }

        if (!string.IsNullOrEmpty(output.RunOutput))
        {
            if (Flags.Debug)
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

    static void UpdateSnapshot(string testFile, string testOutputFile, CompileOutput output, string previousTestOutput)
    {
        StringBuilder testOutput = new();
        testOutput.AppendLine(output.AstOutput);

        string runOutput = output.Success ? Compiler.RunExecutable(output.Executable) : output.Errors;
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

        bool success = output.Success && astOutput == output.AstOutput && runOutput.Trim() == output.RunOutput.Trim();

        string status = success ? "Passed" : "Failed";
        Console.Write(success ? "\x1B[32m" : "\x1B[31m");
        Console.WriteLine($"Test {status}: {testFile}");
        Console.Write("\x1b[0m");

        Terminal.Error(output.Errors);
    }

    static void HandleFlags()
    {
        if (Flags.Help)
        {
            string[] message = [
                "Description:",
                "  b compiler",
                "",
                "Usage:",
                "  bc <source-file> [options]",
                "",
                "Options:",
                "  -t, --target <TARGET>                The target to compile for.",
                "",
                "  --run                                Executes the compiled output.",
                "  --ast                                Outputs the ast view of the parsed file.",
                "  --debug                              Print debug info like stack traces.",
                "  --list-targets                       List all supported targets.",
                "  -h, --help                           Show this help message.",
            ];

            Console.WriteLine(string.Join("\n", message));

            Environment.Exit(0);
        }

        if (Flags.ListTargets)
        {
            string[] message = [
                "Supported targets:",
                "  * qbe (Default)",
            ];

            Console.WriteLine(string.Join("\n", message));

            Environment.Exit(0);
        }
    }
}
