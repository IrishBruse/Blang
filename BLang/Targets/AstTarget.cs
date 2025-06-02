namespace BLang.Targets;

using System;
using BLang.AstParser;

public class AstTarget : BaseTarget
{
    public override string Target => "ast";

    public string Output(CompilationUnit node)
    {
        VisitCompilationUnit(node);

        return output.ToString();
    }

    public void VisitCompilationUnit(CompilationUnit node)
    {
        WriteIndentation();
        WriteLine($"CompilationUnit:");

        Indent();
        foreach (FunctionStatement funcDecl in node.FunctionDeclarations)
        {
            VisitFunctionStatement(funcDecl);
        }
        // TODO: globals
        Dedent();
    }

    public void VisitFunctionStatement(FunctionStatement node)
    {
        WriteIndentation();
        Write($"FunctionDeclaration: {node.FunctionName}(");
        VisitExpressions(node.Parameters);
        WriteLine(")");

        Indent();
        foreach (Statement stmt in node.Body)
        {
            switch (stmt)
            {
                case FunctionStatement s: VisitFunctionStatement(s); break;
                case ExternalStatement s: VisitExternalStatement(s); break;
                case AutoStatement s: VisitAutoStatement(s); break;
                case VariableAssignment s: VisitVariableAssignment(s); break;
                case FunctionCall s: VisitFunctionCall(s); break;
                default: throw new Exception(stmt.ToString());
            }
        }
        Dedent();
    }

    public void VisitExternalStatement(ExternalStatement node)
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
        bool start = true;
        foreach (Expression expr in expressions)
        {
            if (!start)
            {
                Write(", ");
            }
            switch (expr)
            {
                case StringValue e:
                Write($"\"asd\"");
                break;

                case Variable e:
                Write(e.Identifier);
                break;

                case IntValue e:
                Write(e.Value.ToString());
                break;

                default: throw new Exception("" + expr.GetType());
            }
            start = false;
        }
    }

    public void VisitAutoStatement(AutoStatement autoDeclaration)
    {
        WriteIndentation();
        WriteLine($"AutoDeclaration: {string.Join(',', autoDeclaration.Variables)}");
    }

    public void VisitVariableAssignment(VariableAssignment variableAssignment)
    {
        WriteIndentation();
        WriteLine($"VariableAssignment: {variableAssignment.IdentifierName} = {variableAssignment.Value}");
    }

    public void VisitWhileStatement(WhileStatement whileStatement)
    {
        WriteIndentation();
        WriteLine($"WhileDeclaration: {whileStatement}");
    }
}
