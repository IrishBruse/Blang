namespace IBlang;

using System;
using System.Collections.Generic;
using System.IO;
using IBlang.Utility;

public class Program
{
    static readonly Lexer lexer = new(CompilationFlags.None);
    static readonly Parser parser = new();
    static readonly AstPrinter astPrinter = new();

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
        string file;

        if (args.Length == 0)
        {
            file = PickFile();
        }
        else
        {
            file = args[0];
        }

        StreamReader fileStream = File.OpenText(file);
        IEnumerator<Token> tokens = lexer.Lex(fileStream, file);
        CompilationUnit unit = parser.Parse(tokens);

        Console.WriteLine(astPrinter.VisitCompilationUnit(unit));
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
