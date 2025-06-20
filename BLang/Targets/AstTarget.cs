namespace BLang.Targets;

using System;
using System.Collections.Generic;
using System.Linq;
using BLang.AstParser;
using BLang.Utility;

public class AstTarget : BaseTarget
{
    CompilationData data = null!;

    public string Output(CompilationUnit node, CompilationData data)
    {
        this.data = data;

        output.Clear();
        VisitCompilationUnit(node);
        return output.ToString();
    }

    public void VisitCompilationUnit(CompilationUnit node)
    {
        Print(node);

        Indent();
        foreach (FunctionStatement funcDecl in node.FunctionDeclarations)
        {
            VisitFunctionStatement(funcDecl);
        }
        Dedent();
    }

    public void VisitFunctionStatement(FunctionStatement node)
    {
        string parameters = VisitExpressions(node.Parameters);
        Print(node, parameters);

        VisitBlock(node.Body);
    }

    private void VisitBlock(Statement[] statements)
    {
        Indent();
        foreach (Statement stmt in statements)
        {
            switch (stmt)
            {
                case FunctionStatement s: VisitFunctionStatement(s); break;
                case ExternalStatement s: VisitExternalStatement(s); break;
                case AutoStatement s: VisitAutoStatement(s); break;
                case VariableDeclarator s: VisitVariableDeclarator(s); break;
                case FunctionCall s: VisitFunctionCall(s); break;
                case IfStatement s: VisitIfStatement(s); break;
                case WhileStatement s: VisitWhileStatement(s); break;
                default: throw new Exception(stmt.ToString());
            }
        }
        Dedent();
    }

    public void VisitExternalStatement(ExternalStatement node)
    {
        Print(node, string.Join(',', node.Externals.Select(e => e)));
    }

    public void VisitFunctionCall(FunctionCall node)
    {
        string parameters = VisitExpressions(node.Parameters);
        Print(node, parameters);
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
        Print(autoDeclaration, string.Join(',', autoDeclaration.Variables.Select(v => v)));
    }

    public void VisitVariableDeclarator(VariableDeclarator variableAssignment)
    {
        Print(variableAssignment, variableAssignment.Symbol.ToString());
        VisitBinaryExpression(variableAssignment.Value);
    }

    public void VisitBinaryExpression(Expression? expression)
    {
        Indent();
        if (expression is BinaryExpression e)
        {
            Print(e, e.Operation.ToString());
            if (e.Left != null) VisitBinaryExpression(e.Left);
            if (e.Right != null) VisitBinaryExpression(e.Right);
        }
        else if (expression is IntValue iv)
        {
            Print(iv, iv.Value.ToString());
        }
        else if (expression is Variable v)
        {
            Print(v, v.Symbol.ToString());
        }
        Dedent();
    }

    public void VisitWhileStatement(WhileStatement statement)
    {
        Print(statement);
    }

    public void VisitIfStatement(IfStatement statement)
    {
        Print(statement, statement.Condition.ToString());
        VisitBlock(statement.Body);
    }

    public static string GetTypeName(object? obj) => obj?.GetType().ToString().Split(".").Last() ?? "";

    public void Print(AstNode node, string args = "")
    {
        WriteRaw(Space);
        WriteRaw(GetTypeName(node));
        WriteRaw(": ");
        WriteRaw(args + "  -  ");

        if (true)
        {
            string start = data.GetFileLocation(node.Range.Start);
            WriteRaw(start);
        }

        Write();
    }
}
