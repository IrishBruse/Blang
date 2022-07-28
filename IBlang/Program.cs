namespace IBlang;

using System.Diagnostics;

using IBlang.LexerStage;
using IBlang.ParserStage;

public class Program
{
    public static void Main()
    {
        Console.Clear();

        string folder = Path.GetFullPath("../Examples/");

        string[] files = new string[] { "helloworld", "fibonacci" };

        foreach (string item in files)
        {
            string file = Path.Join(folder, item);
            Console.WriteLine("Compiling: " + file);
            Compile(file);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
        }
    }

    private static void Compile(string file)
    {
        string inputFile = file + ".ib";
        string outputFile = file + ".c";

        Context ctx = new(new[] { inputFile }, new());

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
        new Emitter(writer).Visit(ast);
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
                string output = tcc.StandardOutput.ReadToEnd();
                if (!string.IsNullOrEmpty(output))
                {
                    Console.Write(output);
                }

                string error = tcc.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(error))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(error);
                    Console.ResetColor();
                }
            }
        }
    }

    private static Token[] Tokenize(Context ctx)
    {
        Token[] tokens = new Lexer(ctx).Lex();
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
            RedirectStandardError = true,
        });

        if (command == null)
        {
            Log.Error($"{exe} not found in Path");
        }

        return command;
    }
}
