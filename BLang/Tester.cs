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
        long start = DateTime.Now.Millisecond;

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
            UpdateSnapshot(testOutputFile, output, previousTestOutput, start);
        }
        else
        {
            CompareSnapshot(testFile, output, previousTestOutput, start);
        }
    }

    const char IconPass = '✓';
    const char IconFail = (char)215;
    const char IconUpdated = '+';
    const char IconUnchanged = '~';

    static void UpdateSnapshot(string testOutputFile, CompileOutput output, string previousTestOutput, long start)
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


        bool changed = previousTestOutput != newTestOutput;
        if (changed)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(IconUpdated);
            Console.ResetColor();
            long end = DateTime.Now.Millisecond;
            Console.WriteLine($" {testOutputFile} {end - start}ms");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(IconUnchanged);
            Console.ResetColor();
            long end = DateTime.Now.Millisecond;
            Console.WriteLine($" {testOutputFile} {end - start}ms");
        }
    }

    static void CompareSnapshot(string testFile, CompileOutput output, string previousTestOutput, long start)
    {
        string[] parts = previousTestOutput.Split("==============================\n");

        string astOutput = parts[0].Trim();

        bool success = output.Success && string.IsNullOrEmpty(output.Errors) && astOutput == output.AstOutput;
        if (success)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(IconPass);
            Console.ResetColor();
            Console.Write($" {testFile}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            long end = DateTime.Now.Millisecond;
            Console.WriteLine($" {end - start}ms");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(IconFail);
            Console.ResetColor();
            Console.Write($" {testFile}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            long end = DateTime.Now.Millisecond;
            Console.WriteLine($" {end - start}ms");
            Console.ResetColor();
        }

        if (!string.IsNullOrEmpty(output.Errors))
        {
            Error(output.Errors);
        }
    }
}
