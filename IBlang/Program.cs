namespace IBlang;

using System.Diagnostics;

using IBlang.LexerStage;
using IBlang.ParserStage;
using IBlang.TreeEmitterStage;

public class Program
{
    public static void Main(string[] args)
    {
        const string currentFile = @"../Examples/helloworld.ib";
        const string outputFile = "../Examples/test.c";

        Context ctx = new(new[] { currentFile });

        Token[] tokens = new Lexer(ctx).Lex();

        Console.WriteLine();
        Console.WriteLine("-- Tokens Output --");

        foreach (Token token in tokens)
        {
            Console.WriteLine(currentFile + ":" + token);
        }

        Console.WriteLine();
        Console.WriteLine("-- Ast Output --");

        Ast ast = new Parser(ctx, tokens).Parse();
        new Emitter(outputFile).Emit(ast);

        System.Diagnostics.Process.Start(new ProcessStartInfo
        {
            FileName = "clang-format",
            Arguments = outputFile + " -i",
        })?.WaitForExit();

        Console.WriteLine();
        Console.WriteLine("-- Tcc Output --");

        var tcc = System.Diagnostics.Process.Start(new ProcessStartInfo
        {
            FileName = "tcc",
            Arguments = " -run " + outputFile,
            RedirectStandardOutput = true,
        });

        if (tcc == null)
        {
            Console.WriteLine("tcc not found");
            return;
        }

        while (!tcc.HasExited)
        {
            var text = tcc.StandardOutput.ReadToEnd();
            Console.WriteLine(text);
        }
    }
}
