namespace IBlang;

using System;
using System.Collections.Generic;
using System.IO;
using IBlang.AstParser;
using IBlang.Targets;
using IBlang.Tokenizer;
using IBlang.Utility;

public class Compiler
{
    public static void Compile(string? file)
    {
        try
        {
            CompileFile(file);
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(e);
            Console.ResetColor();
        }
    }

    static void CompileFile(string? file)
    {
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

        if (parser.Errors.Count > 0)
        {
            foreach (string error in parser.Errors)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(error);
                Console.ResetColor();
            }
            return;
        }

        if (Flags.Ast)
        {
            Console.WriteLine("==========  AST   ==========");
            astTarget.Output(unit, file);
        }

        string? target = Flags.Target;

        if (target == qbeTarget.Target)
        {
            qbeTarget.Output(unit, file);
        }
        else
        {
            throw new ArgumentException("Unknown target " + target);
        }

    }

    private static void HandleFlags()
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
                "  * ast (Debug)",
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

    public static void DebugTokens(IEnumerator<Token> tokens)
    {
        while (tokens.MoveNext())
        {
            Console.WriteLine(tokens.Current);
        }
    }
}
