namespace BLang;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using BLang.Ast;
using BLang.Targets;
using BLang.Tokenizer;
using BLang.Utility;

public static class Compiler
{
    public static bool TryCompile(string file, [MaybeNullWhen(false)] out CompileOutput output)
    {
        CompilerContext data = new(file);
        output = null;
        List<string> errors = new();

        IEnumerator<Token>? tokens = Lex(file, data);
        if (tokens == null) return false;

        CompilationUnit unit;
        try
        {
            Parser parser = new(data);
            unit = parser.Parse(tokens);
            if (unit == null) return false;
        }
        catch (Exception e)
        {
            errors.Add(e.Message);
            if (Options.Debug) errors.Add(e.StackTrace!);
            return false;
        }


        switch (Options.Target)
        {
            case CompilationTarget.Qbe:
                string? exe = QbeTarget(unit, data);

                if (exe == null)
                {
                    output = null;
                    return false;
                }

                output = new(file, exe, unit, errors.ToArray());
                return true;

            default:
                output = null;
                return false;

        }
    }

    private static string? QbeTarget(CompilationUnit unit, CompilerContext data)
    {
        string file = data.File;

        CreateOutputDirectories(file, "qbe");

        (string objFile, string binFile) = GetOutputFile(file, Targets.QbeTarget.Target);

        string qbeIR = new QbeTarget(data).ToOutput(unit);
        File.WriteAllText(objFile + ".ssa", qbeIR);

        if (!RunExecutable("qbe", $"{objFile}.ssa -o {objFile}.s"))
        {
            // TODO: handle failure of target
            return null;
        }

        if (!RunExecutable("gcc", $"{objFile}.s -o {binFile}"))
        {
            // TODO: handle failure of target
            return null;
        }


        return binFile;
    }

    private static bool RunExecutable(string command, string args)
    {
        Executable exe = Executable.Capture(command, args);
        if (!exe.Success())
        {
            return false;
        }
        return true;
    }

    private static IEnumerator<Token>? Lex(string file, CompilerContext data)
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

    private static CompilationUnit Parse(IEnumerator<Token> tokens, CompilerContext data)
    {
        Parser parser = new(data);
        return parser.Parse(tokens);
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
