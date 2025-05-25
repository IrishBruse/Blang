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
    static readonly Lexer lexer = new(CompilationFlags.None);
    static readonly Parser parser = new();

    static readonly AstTarget astTarget = new();
    static readonly QbeTarget qbeTarget = new();

    public static void Main(string[] args)
    {
        Console.Clear();

        try
        {
            Run(args);
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(e);
            Console.ResetColor();
        }
    }

    public static void Run(string[] args)
    {
        Dictionary<string, string> flags = ParseArgs(args, out string? file);

        file ??= PickFile();

        file = Path.GetFullPath(file);

        StreamReader fileStream = File.OpenText(file);
        IEnumerator<Token> tokens = lexer.Lex(fileStream, file);
        CompilationUnit unit = parser.Parse(tokens);

        string target = flags.GetValueOrDefault("target", "ast").ToLower();

        _ = target switch
        {
            AstTarget.Target => astTarget.Output(unit, file),
            QbeTarget.Target => qbeTarget.Output(unit, file),

            _ => astTarget.Output(unit, file),
        };
    }

    private static Dictionary<string, string> ParseArgs(string[] args, out string? file)
    {
        Dictionary<string, string> flags = [];

        file = null;

        int index = 0;
        while (index < args.Length)
        {
            if (args[index].StartsWith("--"))
            {
                if (args[index + 1].StartsWith("--"))
                {
                    throw new Exception("error parsing args");
                }

                flags.Add(args[index][2..], args[index + 1]);
                index += 2;
            }
            else
            {
                file = args[index];
                index++;
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
