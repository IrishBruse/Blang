namespace IBlang;

using System;
using System.Collections.Generic;
using System.IO;
using IBlang.AstParser;
using IBlang.Targets;
using IBlang.Tokenizer;
using IBlang.Utility;

public class Program
{
    public static void Main(string[] args)
    {
        Console.Clear();

        Flags = ParseArgs(args, out string? file);

        try
        {
            Compile(file);
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(e);
            Console.ResetColor();
        }
    }

    public static void Compile(string? file)
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

        string target = Flags.GetValueOrDefault("target", "qbe").ToLower();

        if (target == astTarget.Target)
        {
            astTarget.Output(unit, file);
        }
        else if (target == qbeTarget.Target)
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
        if (Flags.GetValueOrDefault("help", "") == "true")
        {
            string[] message = [
                "Description:",
                "  b compiler",
                "",
                "Usage:",
                "  bc <source-file> [options]",
                "",
                "Options:",
                "  --run                                Executes the compiled output.",
                "  -t, --target <TARGET>                The target runtime to run for.",
                "  --list-targets                       List all supported targets.",
                "  -h, --help                           Show this help message.",
            ];

            Console.WriteLine(string.Join("\n", message));

            Environment.Exit(0);
        }

        if (Flags.GetValueOrDefault("list-targets", "") == "true")
        {
            string[] message = [
                "Supported targets:",
                "  * qbe (Default)",
                "  * ast (Debug)",
                // "  * gb (TODO)",
                // "  * wasm (TODO)",
                // "  * js (TODO)",
            ];

            Console.WriteLine(string.Join("\n", message));

            Environment.Exit(0);
        }
    }

    private static Dictionary<string, string> ParseArgs(string[] args, out string? file)
    {
        Dictionary<string, string> flags = [];

        file = null;

        int index = 0;
        string? Peek() { return index < args.Length ? args[index] : null; }
        string Next() { return args[index++]; }

        while (index < args.Length)
        {
            string? peek = Peek();
            if (peek != null && peek.StartsWith("--"))
            {
                string flagName = Next()[2..];

                peek = Peek();
                if (peek != null && !peek.StartsWith("--"))
                {
                    flags.Add(flagName, Next());
                }
                else
                {
                    flags.Add(flagName, "true");
                }
            }
            else if (peek != null)
            {
                file = Path.GetFullPath(Next());
            }
            else
            {
                break;
            }
        }

        return flags;
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
