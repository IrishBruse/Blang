namespace IBlang.Targets;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using IBlang.AstParser;
using IBlang.Utility;

public class QbeTarget : BaseTarget, ITargetVisitor
{
    public override string Target => "qbe";

    readonly Dictionary<string, string> strings = [];
    readonly HashSet<string> externs = [];

    int stringIndex = 0;

    public void Output(CompilationUnit node, string file)
    {
        (string objFile, string binFile) = GetOutputFile(file);

        File.Delete(objFile + ".s");
        File.Delete(objFile + ".ssa");
        File.Delete(binFile);

        output = new(new StreamWriter(objFile + ".ssa"));
        VisitCompilationUnit(node);
        output.Dispose();

        Terminal.Debug("==========  QBE   ==========");
        if (GenerateAssembly(objFile) != 0)
        {
            return;
        }

        Terminal.Debug("==========  GCC   ==========");
        if (GenerateExecutable(objFile + ".s", binFile) == 0)
        {
            if (Flags.Run)
            {
                Terminal.Debug("==========  RUN   ==========");
                RunExecutable(binFile);
            }
        }
    }

    private static void RunExecutable(string executable)
    {
        using Process? qbe = Process.Start(executable);
        qbe?.WaitForExit();
    }

    private static int? GenerateAssembly(string output)
    {
        using Process? qbe = Process.Start(new ProcessStartInfo("qbe", output + ".ssa") { RedirectStandardOutput = true, RedirectStandardError = true });
        qbe?.WaitForExit();

        string assemblyText = qbe?.StandardOutput?.ReadToEnd() ?? "";
        string errorOutput = qbe?.StandardError?.ReadToEnd() ?? "";

        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.Write(errorOutput);
        Console.ResetColor();

        File.WriteAllText(output + ".s", assemblyText);

        return qbe?.ExitCode;
    }

    private static int? GenerateExecutable(string outputAssembly, string outputFile)
    {
        string arguments = $"{outputAssembly} -o {outputFile}";
        using Process? gcc = Process.Start(new ProcessStartInfo("gcc", arguments) { RedirectStandardOutput = true, RedirectStandardError = true });
        gcc?.WaitForExit();

        string? standardOutput = gcc?.StandardOutput?.ReadToEnd();
        string? errorOutput = gcc?.StandardError?.ReadToEnd();

        if (!string.IsNullOrEmpty(standardOutput))
        {
            Console.Write(standardOutput);
        }

        if (!string.IsNullOrEmpty(errorOutput))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.Write(errorOutput);
            Console.ResetColor();
        }

        return gcc?.ExitCode;
    }


    public void VisitCompilationUnit(CompilationUnit node)
    {
        foreach (FunctionStatement funcDecl in node.FunctionDeclarations)
        {
            funcDecl.Accept(this);
        }

        foreach (FunctionStatement varDecl in node.VariableDeclarations)
        {
            varDecl.Accept(this);
        }

        GenerateDataSection();
        GenerateExternsSection();
    }

    private void GenerateDataSection()
    {
        WriteLine("# Data");
        foreach ((string value, string name) in strings)
        {
            WriteLine($"data ${name} = {{ b \"{value}\", b 0 }}");
        }
        WriteLine();
    }

    private void GenerateExternsSection()
    {
        WriteLine("# Externs");
        foreach (string external in externs)
        {
            WriteLine($"# * {external}");
        }
        WriteLine();
    }

    public void VisitFunctionDeclaration(FunctionStatement node)
    {
        string returnType = node.FunctionName == "main" ? "w " : "";
        if (node.FunctionName == "main")
        {
            WriteLine($"export function {returnType}${node.FunctionName}()");
        }
        else
        {
            WriteLine($"function w ${node.FunctionName}()");
        }
        BeginScope();
        {
            WriteLine("@start");
            foreach (Statement stmt in node.Statements)
            {
                stmt.Accept(this);
            }

            if (node.FunctionName == "main")
            {
                WriteIndented($"ret 0");
            }
            else
            {
                WriteIndented($"ret");
            }
        }
        EndScope();
        WriteLine();
    }

    public void VisitExternalDeclaration(ExternalStatement node)
    {
        foreach (string extrn in node.Externals)
        {
            externs.Add(extrn);
        }
    }

    public void VisitFunctionCall(FunctionCall node)
    {
        WriteIndentation();
        Write($"call ${node.IdentifierName}");
        Write("(");
        VisitExpressions(node.Parameters);
        WriteLine(")");
    }

    public void VisitExpressions(Expression[] expressions)
    {
        bool start = true;

        foreach (Expression expr in expressions)
        {
            if (!start)
            {
                Write(", ");
            }
            expr.Accept(this);
            start = false;
        }
    }

    public void VisitString(StringValue stringValue)
    {
        if (strings.TryGetValue(stringValue.Value, out string? dataVarName))
        {
            Write($"l ${dataVarName}");
        }
        else
        {
            dataVarName = $"string_{stringIndex++}";
            strings.Add(stringValue.Value, dataVarName);
            Write($"l ${dataVarName}");
        }
    }

    public void VisitInt(IntValue intValue)
    {
        Write("w " + intValue.Value.ToString());
    }

    private void BeginScope()
    {
        WriteLine("{");
        Indent();
    }

    private void EndScope()
    {
        Dedent();
        WriteLine("}");
    }

    public void VisitAutoDeclaration(AutoStatement autoDeclaration)
    {
        foreach (string variable in autoDeclaration.Variables)
        {
            WriteIndented($"%{variable} =l alloc8 8");
        }
        WriteLine("");
    }

    public void VisitVariableAssignment(VariableAssignment variableAssignment)
    {
        // throw new NotImplementedException();
    }
}
