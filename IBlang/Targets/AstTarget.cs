namespace IBlang.Targets;

using System;
using System.IO;
using IBlang.AstParser;

public class AstTarget : BaseTarget, ITargetVisitor
{
    public override string Target => "ast";

    public void Output(CompilationUnit node, string file)
    {
        (string objFile, _) = GetOutputFile(file);
        output = new(Console.Out, new StreamWriter(objFile + ".ast"));

        VisitCompilationUnit(node);

        output.Dispose();
    }

    public void VisitCompilationUnit(CompilationUnit node)
    {
        WriteIndentation();
        WriteLine($"CompilationUnit:");

        Indent();
        foreach (FunctionDeclaration funcDecl in node.FunctionDeclarations)
        {
            funcDecl.Accept(this);
        }
        foreach (FunctionDeclaration varDecl in node.VariableDeclarations)
        {
            varDecl.Accept(this);
        }
        Dedent();
    }

    public void VisitFunctionDeclaration(FunctionDeclaration node)
    {
        WriteIndentation();
        Write($"FunctionDeclaration: {node.FunctionName}(");
        VisitExpressions(node.Parameters);
        WriteLine(")");

        Indent();
        foreach (Statement stmt in node.Statements)
        {
            stmt.Accept(this);
        }
        Dedent();
    }

    public void VisitExternalDeclaration(ExternalStatement node)
    {
        WriteIndentation();
        WriteLine($"ExternalStatement: {node.Externals}");
    }

    public void VisitFunctionCall(FunctionCall node)
    {
        WriteIndentation();
        Write($"FunctionCall: {node.IdentifierName}(");
        VisitExpressions(node.Parameters);
        WriteLine(")");
    }

    public void VisitExpressions(Expression[] expressions)
    {
        foreach (Expression expr in expressions)
        {
            expr.Accept(this);
        }
    }

    public void VisitString(StringValue stringValue)
    {
        Write('"' + stringValue.Value + '"');
    }

    public void VisitInt(IntValue intValue)
    {
        Write(intValue.Value.ToString());
    }
}
