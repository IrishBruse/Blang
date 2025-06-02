namespace BLang;

using System;
using System.Collections.Generic;
using System.IO;
using BLang.AstParser;
using BLang.Exceptions;
using BLang.Targets;
using BLang.Tokenizer;
using BLang.Utility;

public class Compiler
{
    public static CompileOutput Compile(string? file)
    {
        CompileOutput output = new(false);

        try
        {
            output = CompileFile(file);
        }
        catch (ParserException e)
        {
            if (Flags.Instance.Debug)
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
        if (output.Success && Flags.Instance.Run)
        {
            try
            {
                output.RunOutput = Executable.Run(output.Executable);
            }
            catch (Exception)
            {
                // Run
            }
        }

        return output;
    }

    static CompileOutput CompileFile(string? file)
    {
        CompileOutput output = new();

        CompilationData data = new()
        {
            File = file
        };

        output.Success = true;

        Lexer lexer = new(data);
        Parser parser = new(data);

        AstTarget astTarget = new();
        QbeTarget qbeTarget = new();

        file ??= PickFile();

        StreamReader fileStream = File.OpenText(file);

        Terminal.Debug("========== Lexer  ==========");
        IEnumerator<Token> tokens = lexer.Lex(fileStream, file);

        Terminal.Debug("========== Parser ==========");
        CompilationUnit unit = parser.Parse(tokens, file);

        output.Errors += string.Join("\n", parser.Errors);

        if (output.Success)
        {
            output.AstOutput = astTarget.Output(unit);
        }

        string target = Flags.Instance.Target;

        CreateOutputDirectories(file, target);
        (string objFile, string binFile) = GetOutputFile(file, target);

        if (target == qbeTarget.Target)
        {
            string qbeIR = qbeTarget.Output(unit);
            File.WriteAllText(objFile + ".ssa", qbeIR);
            string? qbeAssembly = Executable.Run("qbe", objFile + ".ssa");

            if (qbeAssembly != null)
            {
                File.WriteAllText(objFile + ".s", qbeAssembly);
                string? gccStdOut = Executable.Run("gcc", $"{qbeAssembly} -o {binFile}");
                Console.WriteLine(gccStdOut);
            }
        }
        else
        {
            throw new ArgumentException("Unknown target " + target);
        }

        return output;
    }

    static string PickFile()
    {
        string[] files = Directory.GetFiles("Examples/");
        string[] fileNames = new string[files.Length];

        for (int i = 0; i < files.Length; i++)
        {
            fileNames[i] = Path.GetFileNameWithoutExtension(files[i]);
        }

        int index = Terminal.ShowMenu(fileNames, "Pick Example:\n");

        string file = files[index];
        return file;
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
