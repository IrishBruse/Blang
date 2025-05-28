namespace IBlang;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using IBlang.AstParser;
using IBlang.Targets;
using IBlang.Tokenizer;
using IBlang.Utility;

public class Compiler
{
    public static CompileOutput Compile(string? file)
    {
        CompileOutput output = new(false);

        try
        {
            output = CompileFile(file);
        }
        catch (Exception e)
        {
            output.Errors = e.ToString();
        }

        output.RunOutput = "";
        if (output.Success && Flags.Run)
        {
            output.RunOutput = RunExecutable(output.Executable);
        }

        return output;
    }

    static CompileOutput CompileFile(string? file)
    {
        CompileOutput output = new();

        CompilationData data = new()
        {
            File = file
        };

        Lexer lexer = new(data);
        Parser parser = new(data);

        AstTarget astTarget = new();
        QbeTarget qbeTarget = new();

        HandleFlags();

        file ??= PickFile();

        StreamReader fileStream = File.OpenText(file);
        IEnumerator<Token> tokens = lexer.Lex(fileStream, file);
        CompilationUnit unit = parser.Parse(tokens);

        unit.File = file;

        if (parser.Errors.Count > 0)
        {
            foreach (string error in parser.Errors)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(error);
                Console.ResetColor();
            }
        }

        if (Flags.Ast)
        {
            output.AstOutput = astTarget.Output(unit);
        }

        string? target = Flags.Target;

        if (target == qbeTarget.Target)
        {
            output.Executable = qbeTarget.Output(unit);
        }
        else
        {
            throw new ArgumentException("Unknown target " + target);
        }

        output.Success = true;

        return output;
    }

    public static string RunExecutable(string? executable)
    {
        using Process? qbe = Process.Start(new ProcessStartInfo(executable!) { RedirectStandardOutput = true, RedirectStandardError = true });
        qbe?.WaitForExit();

        string? output = qbe?.StandardOutput.ReadToEnd();
        output += qbe?.StandardError.ReadToEnd();

        return output;
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

    static string PickFile()
    {
        string[] files = Directory.GetFiles("Examples/");
        string[] fileNames = new string[files.Length];

        for (int i = 0; i < files.Length; i++)
        {
            fileNames[i] = Path.GetFileNameWithoutExtension(files[i]);
        }

        int index = Terminal.ShowMenu(fileNames, "Pick Example:\n");

        string file = files[index];
        return file;
    }
}
