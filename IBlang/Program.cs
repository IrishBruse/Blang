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
            string testOutputFile = Path.ChangeExtension(testFile, ".test");

            StringBuilder testOutput = new();
            CompileOutput output = Compiler.Compile(testFile);

            string status = output.Success ? "Passed" : "Failed";
            status = Flags.UpdateSnapshots ? "Updated" : status;
            Console.Write(output.Success ? "\x1B[32m" : "\x1B[31m");
            Console.WriteLine($"Test {status}: {testFile}");
            Console.Write("\x1b[0m");

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

            if (Flags.UpdateSnapshots)
            {
                File.WriteAllText(testOutputFile, testOutput.ToString());
            }
            else
            {
                string test = File.ReadAllText(testOutputFile);

                Console.WriteLine(test);
            }
        }
    }
}
