namespace IBlang.Targets;

using System;
using System.Collections.Generic;
using System.IO;
using IBlang.AstParser;

public class QbeTarget : BaseTarget, IAstVisitor
{
    public const string Target = "qbe";

    readonly Dictionary<string, string> strings = [];
    readonly HashSet<string> externs = [];

    int stringIndex = 0;

    public bool Output(CompilationUnit node, string file)
    {
        string projectDirectory = Path.GetDirectoryName(file)!;
        string sourceFileName = Path.GetFileNameWithoutExtension(file);

        string qbeOutputFile = Path.Combine(projectDirectory, "obj", "qbe", sourceFileName + ".ssa");
        output = new(Console.Out, new StreamWriter(qbeOutputFile));

        VisitCompilationUnit(node);

        return true;
    }

    public void VisitCompilationUnit(CompilationUnit node)
    {
        foreach (FunctionDeclaration funcDecl in node.FunctionDeclarations)
        {
            funcDecl.Accept(this);
        }

        foreach (FunctionDeclaration varDecl in node.VariableDeclarations)
        {
            varDecl.Accept(this);
        }

        GenerateDataSection();
        GenerateExternsSection();
    }

    private void GenerateDataSection()
    {
        WriteLine("# Data");
        foreach ((string name, string value) in strings)
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

    public void VisitFunctionDeclaration(FunctionDeclaration node)
    {
        WriteLine($"export function w ${node.FunctionName}()");
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
        }
        EndScope();
        WriteLine();
    }

    public void VisitExternalDeclaration(ExternalStatement node)
    {
        externs.Add(node.ExternalName);
    }

    public void VisitFunctionCall(FunctionCall node)
    {
        WriteIndentation();
        Write($"call ${node.IdentifierName}");
        Write("(");
        VisitExpressions(node.Parameters);
        Write(")");
        WriteLine();
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
        string dataVarName = $"str_{stringIndex++}";
        strings.Add(dataVarName, stringValue.Value);
        Write($"l ${dataVarName}");
    }

    public void VisitInt(IntValue intValue)
    {
        Write(intValue.Value.ToString());
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
}
