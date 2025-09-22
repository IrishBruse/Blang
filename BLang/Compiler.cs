namespace BLang;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using BLang.Ast;
using BLang.Targets;
using BLang.Tokenizer;
using BLang.Utility;

public static class Compiler
{
    public static bool TryCompile(string file, [MaybeNullWhen(false)] out CompileOutput output)
    {
        // new Dictionary<string, string>().TryGetValue();

        CompilationData data = new(file);

        IEnumerator<Token>? tokens = Lex(file, data);
        if (tokens == null)
        {
            output = null;
            return false;
        }

        CompilationUnit? unit = Parse(tokens, data);
        if (unit == null)
        {
            output = null;
            return false;
        }

        switch (Options.Target)
        {
            case CompilationTarget.Qbe:
                output = QbeTarget(unit, data);
                return true;

            default:
                output = null;
                return false;

        }

        // if (Options.Ast)
        // {
        //     output.AstOutput = GenerateAstJson(unit);
        //     string astFile = Path.ChangeExtension(file, "ast");
        //     File.WriteAllText(astFile, output.AstOutput);
        // }
    }

    private static CompileOutput QbeTarget(CompilationUnit unit, CompilationData data)
    {
        string file = data.File;

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

    private static IEnumerator<Token>? Lex(string file, CompilationData data)
    {
        try
        {
            Lexer lexer = new(data);
            return lexer.Lex(File.OpenText(file), file);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Lex error: " + e);
            return null;
        }
    }

    private static CompilationUnit? Parse(IEnumerator<Token> tokens, CompilationData data)
    {
        try
        {
            Parser parser = new(data);
            return parser.Parse(tokens);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(Colors.Red(e.Message));
            if (Options.Debug)
            {
                Console.Error.WriteLine(Colors.Red(e.StackTrace));
            }
            return null;
        }
    }

    private static string GenerateAstJson(CompilationUnit unit)
    {
        JsonTypeInfo<CompilationUnit> jsonOptions = Options.Verb == Verb.Test ? CompilationUnitTestContext.Default.CompilationUnit : CompilationUnitContext.Default.CompilationUnit;
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

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(CompilationUnit))]
public partial class CompilationUnitContext : JsonSerializerContext;

[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
[JsonSerializable(typeof(CompilationUnit))]
public partial class CompilationUnitTestContext : JsonSerializerContext;
