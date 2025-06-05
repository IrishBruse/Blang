namespace BLang.Targets;

using System;
using System.Collections.Generic;
using BLang.AstParser;
using BLang.Exceptions;
using BLang.Tokenizer;
using BLang.Utility;

public class QbeTarget(CompilationData data) : BaseTarget
{
    public const string Target = "qbe";
    public override int Indention => 4;

    readonly Dictionary<string, string> strings = [];
    readonly HashSet<string> externs = [];

    readonly SymbolTable symbols = data.Symbols;

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
            VisitFunctionStatement(funcDecl);
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

    public void VisitStatement(Statement node)
    {
        switch (node)
        {
            case ExternalStatement s: VisitExternalStatement(s); break;
            case FunctionCall s: VisitFunctionCall(s); break;
            case AutoStatement s: VisitAutoStatement(s); break;
            case VariableAssignment s: VisitVariableAssignment(s); break;
            case WhileStatement s: VisitWhileStatement(s); break;
            default: throw new Exception(node.ToString());
        }
    }

    private void BeginScope()
    {
        WriteLine("{");
        Indent();
        symbols.EnterScope();
    }

    private void EndScope()
    {
        symbols.ExitScope();
        Dedent();
        WriteLine("}");
    }

    // Statements

    public void VisitFunctionStatement(FunctionStatement node)
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
                VisitStatement(stmt);
            }

            if (node.FunctionName == "main")
            {
                Write($"ret 0");
            }
            else
            {
                Write($"ret");
            }
        }
        EndScope();
        WriteLine();
    }

    public void VisitExternalStatement(ExternalStatement node)
    {
        foreach (string extrn in node.Externals)
        {
            externs.Add(extrn);
        }
    }

    public void VisitWhileStatement(WhileStatement whileStatement)
    {
        Write($"VisitWhileDeclaration {whileStatement}");
    }

    public void VisitAutoStatement(AutoStatement autoDeclaration)
    {
        foreach (string variable in autoDeclaration.Variables)
        {
            Write($"%{variable} =l alloc4 4");
        }
        WriteLine();
    }

    public void VisitFunctionCall(FunctionCall node)
    {
        string parameters = LoadParameterValue(node.Parameters);
        Write($"call ${node.IdentifierName}({parameters})");
        WriteLine();
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
                Write($"{tmpReg} =w loadw %{v.Identifier}");
                registers.Add($"w {tmpReg}");
                break;

                default: throw new ParserException($"Unknown parameter {expr.GetType()}({expr})");
            }
        }

        return string.Join(", ", registers);
    }

    public void VisitVariableAssignment(VariableAssignment variableAssignment)
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

    public void VisitBinaryExpression(BinaryExpression binary, string regName)
    {
        switch (binary.Operation)
        {
            case TokenType.None:
            Write($"storew {binary?.Left}, {regName}");
            break;

            case TokenType.Addition:
            Write($"storew {binary?.Left}, {regName}");
            break;
        }

        WriteLine();
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
