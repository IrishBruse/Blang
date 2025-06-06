namespace BLang;

using System;
using System.Collections.Generic;
using System.IO;
using BLang.AstParser;
using BLang.Targets;
using BLang.Tokenizer;
using BLang.Utility;

public static class Compiler
{
    static readonly AstTarget astPrinter = new();

    public static CompileOutput Compile(string file)
    {
        CompileOutput output = CompileFile(file);

        if (output.Success && options.Run)
        {
            try
            {
                Executable.Run(output.Executable);
            }
            catch (Exception e)
            {
                output.Errors = e.Message;
            }
        }

        return output;
    }

    static CompileOutput CompileFile(string file)
    {
        CompileOutput output = new();
        CompilationData data = new(file);
        Lexer lexer = new(data);
        Parser parser = new(data);

        IEnumerator<Token> tokens = lexer.Lex(File.OpenText(file), file);
        CompilationUnit unit = parser.Parse(tokens);

        string ast = astPrinter.Output(unit);
        Header("AST");
        Log(ast);

        string target = options.Target;

        CreateOutputDirectories(file, target);
        (string objFile, string binFile) = GetOutputFile(file, target);

        if (target == QbeTarget.Target)
        {
            QbeTarget qbeTarget = new(data);

            string qbeIR = qbeTarget.Output(unit);
            File.WriteAllText(objFile + ".ssa", qbeIR);

            Executable exe = Executable.Capture("qbe", objFile + ".ssa");
            if (!exe.Success())
            {
                output.Success = false;
            }
            Error(exe.StdError, "ERR");

            if (exe.Success())
            {
                string assemblyFile = objFile + ".s";
                File.WriteAllText(assemblyFile, exe.StdOut);

                exe = Executable.Capture("gcc", $"{assemblyFile} -o {binFile}");
                if (!exe.Success())
                {
                    output.Success = false;
                }
                Error(exe.StdError, "ERR");
            }
        }
        else
        {
            throw new ArgumentException("Unknown target " + target);
        }

        output.Executable = binFile;

        return output;
    }

    static (string, string) GetOutputFile(string inputFile, string target)
    {
        string projectDirectory = Path.GetDirectoryName(inputFile)!;
        string sourceFileName = Path.GetFileNameWithoutExtension(inputFile);
        string objFile = Path.Combine(projectDirectory, "obj", target, sourceFileName);
        string binFile = Path.Combine(projectDirectory, "bin", target, sourceFileName);

        return (objFile, binFile);
    }

    static void CreateOutputDirectories(string inputFile, string target)
    {
        string projectDirectory = Path.GetDirectoryName(inputFile)!;

        Directory.CreateDirectory(Path.Combine(projectDirectory, "obj", target));
        Directory.CreateDirectory(Path.Combine(projectDirectory, "bin", target));
    }
}
