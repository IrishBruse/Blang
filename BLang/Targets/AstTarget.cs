namespace BLang.Targets;

using System;
using System.Collections.Generic;
using System.Linq;
using BLang.AstParser;

public class AstTarget : BaseTarget
{
    public string Output(CompilationUnit node)
    {
        output.Clear();
        VisitCompilationUnit(node);
        return output.ToString();
    }

    public void VisitCompilationUnit(CompilationUnit node)
    {
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
        string parameters = VisitExpressions(node.Parameters);
        WriteLine($"FunctionDeclaration: {node.Symbol}: {parameters}");

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
        WriteLine($"ExternalStatement: {string.Join(',', node.Externals.Select(e => e))}");
    }

    public void VisitFunctionCall(FunctionCall node)
    {
        string parameters = VisitExpressions(node.Parameters);
        WriteLine($"FunctionCall: {node.Symbol}: {parameters}");
    }

    static string VisitExpressions(Expression[] expressions)
    {
        List<string> expression = [];
        foreach (Expression expr in expressions)
        {
            switch (expr)
            {
                case StringValue e:
                expression.Add($"\"{e.Value}\"");
                break;

                case Variable e:
                expression.Add(e.Symbol.ToString());
                break;

                case IntValue e:
                expression.Add(e.Value.ToString());
                break;

                default: throw new Exception("" + expr.GetType());
            }
        }

        return string.Join(", ", expression);
    }

    public void VisitAutoStatement(AutoStatement autoDeclaration)
    {
        WriteLine($"AutoDeclaration: {string.Join(',', autoDeclaration.Variables.Select(v => v))}");
    }

    public void VisitVariableAssignment(VariableAssignment variableAssignment)
    {
        WriteLine($"VariableAssignment: {variableAssignment.Symbol}");
        VisitBinaryExpression(variableAssignment.Value);
    }

    public void VisitBinaryExpression(Expression? expression)
    {
        Indent();
        if (expression is BinaryExpression e)
        {
            WriteLine($"{GetTypeName(e)}: {e.Operation}");
            if (e.Left != null) VisitBinaryExpression(e.Left);
            if (e.Right != null) VisitBinaryExpression(e.Right);
        }
        else if (expression is IntValue iv)
        {
            WriteLine($"{GetTypeName(iv)}: {iv.Value}");
        }
        else if (expression is Variable v)
        {
            WriteLine($"{GetTypeName(v)}: {v.Symbol}");
        }
        Dedent();
    }

    public void VisitWhileStatement(WhileStatement whileStatement)
    {
        WriteLine($"WhileDeclaration: {whileStatement}");
    }

    public static string GetTypeName(object? obj) => obj?.GetType().ToString().Split(".").Last() ?? "";
}
