namespace IBlang;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using IBlang.AstParser;
using IBlang.Targets;
using IBlang.Tokenizer;
using IBlang.Utility;

public class Compiler
{
    public static CompileOutput Compile(string? file)
    {
        CompileOutput output = new(false);

        try
        {
            output = CompileFile(file);
        }
        catch (Exception e)
        {
            output.Errors = e.ToString();
        }

        output.RunOutput = "";
        if (output.Success && Flags.Run)
        {
            output.RunOutput = RunExecutable(output.Executable);
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

        Lexer lexer = new(data);
        Parser parser = new(data);

        AstTarget astTarget = new();
        QbeTarget qbeTarget = new();

        file ??= PickFile();

        StreamReader fileStream = File.OpenText(file);
        IEnumerator<Token> tokens = lexer.Lex(fileStream, file);
        CompilationUnit unit = parser.Parse(tokens, file);

        foreach (string error in parser.Errors)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            Console.ResetColor();
        }

        if (Flags.Ast)
        {
            output.AstOutput = astTarget.Output(unit);
        }

        string? target = Flags.Target;

        if (target == qbeTarget.Target)
        {
            output.Executable = qbeTarget.Output(unit);
        }
        else
        {
            throw new ArgumentException("Unknown target " + target);
        }

        output.Success = true;

        return output;
    }

    public static string RunExecutable(string? executable)
    {
        using Process? qbe = Process.Start(new ProcessStartInfo(executable!) { RedirectStandardOutput = true, RedirectStandardError = true });
        qbe?.WaitForExit();

        string? output = qbe?.StandardOutput.ReadToEnd();
        output += qbe?.StandardError.ReadToEnd();

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
}
