namespace IBlang.Targets;

using System;
using IBlang.AstParser;

public class AstTarget : BaseTarget, ITargetVisitor
{
    public override string Target => "ast";

    public void Output(CompilationUnit node, string file)
    {
        (_, _) = GetOutputFile(file);
        output = new(Console.Out);

        VisitCompilationUnit(node);

        output.Dispose();
    }

    public void VisitCompilationUnit(CompilationUnit node)
    {
        WriteIndentation();
        WriteLine($"CompilationUnit:");

        Indent();
        foreach (FunctionStatement funcDecl in node.FunctionDeclarations)
        {
            funcDecl.Accept(this);
        }
        foreach (FunctionStatement varDecl in node.VariableDeclarations)
        {
            varDecl.Accept(this);
        }
        Dedent();
    }

    public void VisitFunctionDeclaration(FunctionStatement node)
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

    public void VisitAutoDeclaration(AutoStatement autoDeclaration)
    {
        throw new NotImplementedException();
    }

    public void VisitVariableAssignment(VariableAssignment variableAssignment)
    {
        throw new NotImplementedException();
    }
}
