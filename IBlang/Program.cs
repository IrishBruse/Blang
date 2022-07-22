namespace IBlang;

using System.Diagnostics;

using IBlang.LexerStage;
using IBlang.ParserStage;

public class Program
{
    public static void Main()
    {
        string currentFile = Path.GetFullPath("../Examples/helloworld.ib");
        string outputFile = Path.GetFullPath(currentFile.Replace(".ib", ".c"));

        Context ctx = new(new[] { currentFile });

        Token[] tokens = Tokenize(ctx);
        Ast ast = Parse(ctx, tokens);
        Emit(outputFile, ast);
        ClangFormat(outputFile);
        RunWithTcc(outputFile);
    }

    private static void ClangFormat(string outputFile)
    {
        Process? clangfmt = RunCommand("clang-format", outputFile, "-i");
        if (clangfmt != null)
        {
            while (!clangfmt.HasExited)
            {
                string value = clangfmt.StandardOutput.ReadToEnd();
                if (!string.IsNullOrEmpty(value))
                {
                    Console.WriteLine(value);
                }
            }
        }
    }

    private static void Emit(string outputFile, Ast ast)
    {
        File.Delete(path: outputFile);
        StreamWriter writer = new(File.OpenWrite(outputFile));
        new Visitor(writer).Visit(ast);
        writer.Close();
        Console.WriteLine();
    }

    private static Ast Parse(Context ctx, Token[] tokens)
    {
        Ast ast = new Parser(ctx, tokens).Parse();
        return ast;
    }

    private static void RunWithTcc(string outputFile)
    {
        Process? tcc = RunCommand("tcc", "-run", outputFile);

        Console.WriteLine();
        if (tcc != null)
        {
            while (!tcc.HasExited)
            {
                string text = tcc.StandardOutput.ReadToEnd();
                if (text.StartsWith("tcc: error:"))
                {
                    Log.Error(text);
                }
                else
                {
                    Console.Write(text);
                }
            }
        }
    }

    private static Token[] Tokenize(Context ctx)
    {
        Token[] tokens = new Lexer(ctx).Lex();

        foreach (Token token in tokens)
        {
            Console.WriteLine(token);
        }

        Console.WriteLine();
        return tokens;
    }

    private static Process? RunCommand(string exe, params string[] args)
    {
        Console.WriteLine($"> {exe} {string.Join(' ', args)}");

        Process? command = Process.Start(new ProcessStartInfo
        {
            FileName = exe,
            Arguments = string.Join(' ', args),
            RedirectStandardOutput = true,
        });

        if (command == null)
        {
            Log.Error($"{exe} not found in Path");
        }

        return command;
    }
}
