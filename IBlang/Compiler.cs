namespace IBlang;

using System.Diagnostics;
using System.Text;

using IBlang.Stage1Lexer;
using IBlang.Stage2Parser;
using IBlang.Stage3TypeChecker;
using IBlang.Stage4Lowering;
using IBlang.Stage5CodeGen;

public class Compiler
{
    public static void CompileAndRunFile(string sourceFile)
    {
        string outputFile = OutFile(sourceFile, ".c");
        Compile(sourceFile);
        Command.Run("tcc", "-run", outputFile);
    }

    public static void CompileFile(string sourceFile)
    {
        string outputFile = OutFile(sourceFile, ".c");
        Compile(sourceFile);
        Command.Run("tcc", outputFile, "-o", OutFile(sourceFile, ".exe"));
    }

    public static (string output, string expected) Test(string sourceFile)
    {
        string outputFile = OutFile(sourceFile, ".c");
        Compile(sourceFile);

        Process command = Process.Start(new ProcessStartInfo
        {
            FileName = "tcc",
            Arguments = string.Join(' ', "-run", outputFile),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        })!;

        command.WaitForExit();

        StringBuilder output = new();

        do
        {
            _ = output.Append(command.StandardOutput.ReadToEnd());
            _ = output.Append(command.StandardError.ReadToEnd());
        }
        while (!command.HasExited);

        string expected = File.ReadAllText(sourceFile + ".out");

        return (output.ToString(), expected);
    }

    private static void Compile(string sourceFile)
    {
        string outputFile = OutFile(sourceFile, ".c");

        Context ctx = new(sourceFile, new());

        Token[] tokens = new Lexer(ctx).Lex(sourceFile);
        Ast ast = new Parser(ctx).Parse(tokens);
        ast = new TypeChecker(ctx).TypeCheck(ast);
        ast = new AstLowering(ctx).Lower(ast);
        StringBuilder cSource = new CEmitter().Emit(ast);

        File.WriteAllText(outputFile, cSource.ToString());
    }

    public static void Format(string sourceFile)
    {
        string outputFile = OutFile(sourceFile, ".c");
        Command.Run("clang-format", "-i", outputFile);
    }

    private static string OutFile(string sourceFile, string ext)
    {
        return sourceFile.Replace(".ib", ext);
    }
}
