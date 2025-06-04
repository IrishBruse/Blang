namespace BLang.Targets;

using System;
using System.Collections.Generic;
using System.Linq;
using BLang.AstParser;
using BLang.Exceptions;
using BLang.Tokenizer;

public class QbeTarget : BaseTarget
{
    public override string Target => "qbe";

    readonly Dictionary<string, string> strings = [];
    readonly HashSet<string> externs = [];

    int tempRegistry = 0;

    public string Output(CompilationUnit node)
    {
        VisitCompilationUnit(node);
        return output.ToString();
    }

    public void VisitCompilationUnit(CompilationUnit node)
    {
        foreach (FunctionStatement funcDecl in node.FunctionDeclarations)
        {
            Visit(funcDecl);
        }

        // TODO: global variables

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

    public void Visit(Statement node)
    {
        switch (node)
        {
            case ExternalStatement s: Visit(s); break;
            case FunctionCall s: VisitFunctionCall(s); break;
            case AutoStatement s: Visit(s); break;
            case VariableAssignment s: Visit(s); break;
            case WhileStatement s: Visit(s); break;
            default: throw new Exception(node.ToString());
        }
    }

    public string VisitParameters(Expression[] expressions)
    {
        string[] parameters = expressions.Select(expr =>
        {
            return expr switch
            {
                StringValue e => $"\"{e.Value}\"",
                IntValue e => e.Value.ToString(),
                Variable e => e.Identifier,
                _ => throw new Exception(expr.GetType().ToString())
            };
        }).ToArray();

        return string.Join(", ", parameters);
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

    // Statements

    public void Visit(FunctionStatement node)
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
            foreach (Statement stmt in node.Body)
            {
                Visit(stmt);
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

    public void Visit(ExternalStatement node)
    {
        foreach (string extrn in node.Externals)
        {
            externs.Add(extrn);
        }
    }

    public void Visit(WhileStatement whileStatement)
    {
        WriteIndented($"VisitWhileDeclaration");
    }

    public void Visit(AutoStatement autoDeclaration)
    {
        foreach (string variable in autoDeclaration.Variables)
        {
            WriteIndented($"%{variable} =l alloc4 4");
        }
        WriteLine("");
    }

    public void VisitFunctionCall(FunctionCall node)
    {
        string parameters = LoadParameterValue(node.Parameters);
        WriteIndented($"call ${node.IdentifierName}({parameters})");
    }

    string LoadParameterValue(Expression[] parameters)
    {
        List<string> registers = [];
        foreach (AstNode expr in parameters)
        {
            string tmpReg;
            switch (expr)
            {
                case StringValue v:
                tmpReg = TempRegister();
                string reg = VisitStringValue(v);
                registers.Add($"l ${reg}");
                break;

                case Variable v:
                tmpReg = TempRegister(v.Identifier);
                WriteIndented($"{tmpReg} =w loadw %{v.Identifier}");
                registers.Add($"w {tmpReg}");
                break;

                default: throw new ParserException($"Unknown parameter {expr.GetType()}({expr})");
            }
        }

        return string.Join(", ", registers);
    }

    public void Visit(VariableAssignment variableAssignment)
    {
        BinaryExpression value = variableAssignment.Value;
        VisitBinaryExpression(value, $"%{variableAssignment.IdentifierName}");
    }

    // Expression

    public string VisitStringValue(StringValue stringValue)
    {
        if (strings.TryGetValue(stringValue.Value, out string? dataVarName))
        {
            return dataVarName;
        }
        else
        {
            dataVarName = $"string_{strings.Count}";
            strings.Add(stringValue.Value, dataVarName);
            return dataVarName;
        }
    }

    public void VisitIntValue(IntValue intValue)
    {
        Write($"w {intValue.Value}");
    }

    public void VisitVariable(Variable variableValue)
    {
        Write($"w %{variableValue.Identifier}_parameter");
    }

    public void VisitBinaryExpression(BinaryExpression binary, string regName)
    {
        switch (binary.Operation)
        {
            case TokenType.None:
            WriteIndented($"storew {binary?.Left}, {regName}");
            break;

            case TokenType.Addition:
            WriteIndented($"storew {binary?.Left}, {regName}");
            break;
        }
    }

    // Utility

    public string TempRegister(string friendlyName = "")
    {
        string reg = $"%{friendlyName}_{tempRegistry}";
        tempRegistry++;
        return reg;
    }
    public string GlobalRegister() => $"$string_{strings.Count}";
}
