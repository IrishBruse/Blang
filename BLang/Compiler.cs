namespace BLang;

using System;
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
        Lexer lexer = new(data);
        Parser parser = new(data);

        IEnumerator<Token> tokens = lexer.Lex(File.OpenText(file), file);
        CompilationUnit unit = parser.Parse(tokens);

        string target = Options.Target;

        CreateOutputDirectories(file, target);
        (string objFile, string binFile) = GetOutputFile(file, target);

        JsonSerializerOptions jsonOptions = Options is TestOptions ? TestAstJsonOptions : AstJsonOptions;
        string astJson = JsonSerializer.Serialize(unit, jsonOptions);
        output.AstOutput = astJson;
        Debug(output.AstOutput);

        if (target == QbeTarget.Target)
        {
            QbeTarget qbeTarget = new(data);

            string qbeIR = qbeTarget.ToOutput(unit);
            File.WriteAllText(objFile + ".ssa", qbeIR);

            Executable exe = Executable.Capture("qbe", $"{objFile}.ssa -o {objFile}.s");
            if (!exe.Success())
            {
                Error(exe.StdError, "ERR");
                output.Success = false;
                return output;
            }

            exe = Executable.Capture("gcc", $"{objFile}.s -g -o {binFile}");
            if (!exe.Success())
            {
                Error(exe.StdError, "ERR");
                output.Success = false;
                return output;
            }
        }
        else
        {
            throw new ArgumentException("Unknown target " + target);
        }

        output.Executable = binFile;

        return output;
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
