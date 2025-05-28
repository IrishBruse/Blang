namespace IBlang.Targets;

using System;
using System.IO;
using System.Text;
using IBlang.AstParser;

public class AstTarget : BaseTarget, ITargetVisitor
{
    public override string Target => "ast";

    public string Output(CompilationUnit node)
    {
        StringBuilder capturedOutput = new();
        StringWriter stringWriter = new(capturedOutput);

        output = new(stringWriter);

        VisitCompilationUnit(node);

        return capturedOutput.ToString().Trim();
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
        WriteLine($"ExternalStatement: {string.Join(',', node.Externals)}");
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
        WriteIndentation();
        WriteLine($"AutoDeclaration: {string.Join(',', autoDeclaration.Variables)}");
    }

    public void VisitVariableAssignment(VariableAssignment variableAssignment)
    {
        WriteIndentation();
        WriteLine($"VariableAssignment: {variableAssignment.IdentifierName} = {variableAssignment.Value}");
    }
}
