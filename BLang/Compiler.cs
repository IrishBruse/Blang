namespace BLang;

using System;
using System.Collections.Generic;
using System.IO;
using BLang.AstParser;
using BLang.Exceptions;
using BLang.Targets;
using BLang.Tokenizer;
using BLang.Utility;

public class Compiler(Options options)
{
    public CompileOutput Compile(string file)
    {
        CompileOutput output = new(false);

        try
        {
            output = CompileFile(file);
        }
        catch (ParserException e)
        {
            if (options.Debug)
            {
                output.Errors += e.ToString();
            }
            else
            {
                output.Errors += e.Error;
            }
            output.Success = false;
        }
        catch (Exception)
        {
            output.Success = false;
            throw;
        }

        output.RunOutput = "";
        if (output.Success && options.Run)
        {
            try
            {
                Executable.Run(output.Executable).Switch(success =>
                {
                    output.RunOutput = success;
                },
                error =>
                {
                    output.Errors = error.Value;
                });
            }
            catch (Exception e)
            {
                output.Errors = e.Message;
            }
        }

        return output;
    }

    CompileOutput CompileFile(string file)
    {
        CompileOutput output = new();

        CompilationData data = new()
        {
            File = file
        };

        output.Success = true;

        Lexer lexer = new(data, options);
        Parser parser = new(data, options);

        AstTarget astTarget = new();
        QbeTarget qbeTarget = new();

        StreamReader fileStream = File.OpenText(file);

        IEnumerator<Token> tokens = lexer.Lex(fileStream, file);

        CompilationUnit unit = parser.Parse(tokens, file);

        output.AstOutput = astTarget.Output(unit);

        string target = options.Target;

        CreateOutputDirectories(file, target);
        (string objFile, string binFile) = GetOutputFile(file, target);

        if (target == qbeTarget.Target)
        {
            string qbeIR = qbeTarget.Output(unit);
            File.WriteAllText(objFile + ".ssa", qbeIR);

            Executable.Run("qbe", objFile + ".ssa").Switch(qbeAssembly =>
            {
                string assemblyFile = objFile + ".s";
                File.WriteAllText(assemblyFile, qbeAssembly);

                string? gccStdOut = Executable.Run("gcc", $"{assemblyFile} -o {binFile}").Catch(error =>
                {
                    Error(error.Value, "ERR");
                });

                Log(gccStdOut, "OUT");
            }, error =>
            {
                Error(error.Value, "ERR");
            });
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

    void Debug(string message)
    {
        if (options.Debug)
        {
            Console.WriteLine(message);
        }
    }
}
