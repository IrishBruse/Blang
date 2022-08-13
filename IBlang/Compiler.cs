namespace IBlang;

using System.Diagnostics;
using System.Text;

using IBlang.Stage1Lexer;
using IBlang.Stage2Parser;
using IBlang.Stage4CodeGen;

public class Compiler
{
    public static void CompileAndRun(string sourceFile)
    {
        string outputFile = OutFile(sourceFile, ".c");
        Output(sourceFile);
        Command.Run("tcc", "-run", outputFile);
    }

    public static void Compile(string sourceFile)
    {
        string outputFile = OutFile(sourceFile, ".c");
        Output(sourceFile);
        Command.Run("tcc", outputFile, "-o", OutFile(sourceFile, ".exe"));
    }

    public static bool Test(string sourceFile)
    {
        string outputFile = OutFile(sourceFile, ".c");
        Output(sourceFile);
        Process? command = Command.Start("tcc", "-run", outputFile);

        if (command == null)
        {
            return false;
        }

        StringBuilder builder = new();

        while (command.HasExited)
        {
            string output = command.StandardOutput.ReadToEnd();
            if (!string.IsNullOrEmpty(output))
            {
                _ = builder.Append(output);
            }
        }

        Console.WriteLine(builder);

        if (File.ReadAllText(sourceFile + ".out") == builder.ToString())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private static void Output(string sourceFile)
    {
        string outputFile = OutFile(sourceFile, ".c");

        Context ctx = new(new[] { sourceFile }, new());

        Token[] tokens = new Lexer(ctx).Lex(sourceFile);
        Ast node = new Parser(ctx).Parse(tokens);

        StringBuilder cSource = new CEmitter(ctx).Emit(node);

        File.WriteAllText(outputFile, cSource.ToString());
    }

    public static void Format(string sourceFile)
    {
        string outputFile = OutFile(sourceFile, ".c");
        Command.Run("clang-format", "-i", outputFile);
    }

    private static string OutFile(string sourceFile, string ext) => sourceFile.Replace(".ib", ext);
}
