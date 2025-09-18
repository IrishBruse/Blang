namespace BLang;

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using BLang.Ast;
using BLang.Ast.Nodes;
using BLang.Targets;
using BLang.Tokenizer;
using BLang.Utility;

public static class Compiler
{
    private static JsonSerializerOptions AstJsonOptions = new() { WriteIndented = true };
    private static JsonSerializerOptions TestAstJsonOptions = new() { WriteIndented = true, IncludeFields = true };

    public static CompileOutput Compile(string file)
    {
        CompileOutput output = new();
        CompilationData data = new(file);

        IEnumerator<Token> tokens = LexFile(file, data);
        CompilationUnit unit = ParseTokens(tokens, data);

        if (Options.Target == CompilationTarget.Qbe)
        {
            output = QbeTarget(file, data, unit);
        }

        output.AstOutput = GenerateAstJson(unit);

        return output;
    }

    private static CompileOutput QbeTarget(string file, CompilationData data, CompilationUnit unit)
    {
        CreateOutputDirectories(file, "qbe");

        (string objFile, string binFile) = GetOutputFile(file, Targets.QbeTarget.Target);
        CompileOutput output = new();

        string qbeIR = new QbeTarget(data).ToOutput(unit);
        File.WriteAllText(objFile + ".ssa", qbeIR);

        if (!RunExecutable("qbe", $"{objFile}.ssa -o {objFile}.s", output))
        {
            return output;
        }

        if (!RunExecutable("gcc", $"{objFile}.s -o {binFile}", output))
        {
            return output;
        }

        output.Executable = binFile;
        output.Success = true;

        return output;
    }

    private static bool RunExecutable(string command, string args, CompileOutput output)
    {
        Executable exe = Executable.Capture(command, args);
        if (!exe.Success())
        {
            Error(exe.StdError, "ERR");
            output.Success = false;
            return false;
        }
        return true;
    }

    private static IEnumerator<Token> LexFile(string file, CompilationData data)
    {
        Lexer lexer = new(data);
        return lexer.Lex(File.OpenText(file), file);
    }

    private static CompilationUnit ParseTokens(IEnumerator<Token> tokens, CompilationData data)
    {
        Parser parser = new(data);
        return parser.Parse(tokens);
    }

    private static string GenerateAstJson(CompilationUnit unit)
    {
        JsonSerializerOptions jsonOptions = Options.Verb == Verb.Test ? TestAstJsonOptions : AstJsonOptions;
        return JsonSerializer.Serialize(unit, jsonOptions);
    }

    private static (string, string) GetOutputFile(string inputFile, string target)
    {
        string projectDirectory = Path.GetDirectoryName(inputFile)!;
        string sourceFileName = Path.GetFileNameWithoutExtension(inputFile);
        string objFile = Path.Combine(projectDirectory, "obj", target, sourceFileName);
        string binFile = Path.Combine(projectDirectory, "bin", target, sourceFileName);

        return (objFile, binFile);
    }

    private static void CreateOutputDirectories(string inputFile, string target)
    {
        string projectDirectory = Path.GetDirectoryName(inputFile)!;

        _ = Directory.CreateDirectory(Path.Combine(projectDirectory, "obj", target));
        _ = Directory.CreateDirectory(Path.Combine(projectDirectory, "bin", target));
    }
}
