namespace BLang;

using System;
using System.Collections.Generic;
using System.IO;
using BLang.Ast;
using BLang.Targets;
using BLang.Tokenizer;
using BLang.Utility;

public static class Compiler
{
    public static Result<CompileOutput> Compile(string file)
    {
        CompilerContext data = new(file);

        Result<IEnumerator<Token>> tokens = Lex(file, data);
        if (!tokens) return tokens.Error;

        CompilationUnit unit;
        try
        {
            Parser parser = new(data);
            unit = parser.Parse(tokens.Value);
            if (unit == null) return "Failed to parse " + file;
        }
        catch (Exception e)
        {
            return Options.Verbose > 0 ? e.ToString() : e.Message;
        }

        return Options.Target switch
        {
            CompilationTarget.Qbe => QbeTarget(unit, data),
            _ => $"Unknown Target {Options.Target}",
        };
    }

    private static Result<CompileOutput> QbeTarget(CompilationUnit unit, CompilerContext data)
    {
        string file = data.File;

        CreateOutputDirectories(file, "qbe");

        (string objFile, string binFile) = GetOutputFile(file, Targets.QbeTarget.Target);

        string qbeIR = new QbeTarget(data).ToOutput(unit);
        File.WriteAllText(objFile + ".ssa", qbeIR);

        if (!RunExecutable("qbe", $"{objFile}.ssa -o {objFile}.s"))
        {
            return "Failed to compile qbe ir to assembly";
        }

        if (!RunExecutable("gcc", $"{objFile}.s -o {binFile}"))
        {
            return "Failed to compile converted assembly using gcc";
        }

        return new CompileOutput(file, binFile, unit);
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

    private static Result<IEnumerator<Token>> Lex(string file, CompilerContext data)
    {
        try
        {
            Lexer lexer = new(data);
            IEnumerator<Token> test = lexer.Lex(File.OpenText(file), file);
            return new Result<IEnumerator<Token>>(test);
        }
        catch (Exception e)
        {
            return Options.Verbose > 0 ? e.ToString() : e.Message;
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
