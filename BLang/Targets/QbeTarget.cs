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
    readonly HashSet<Symbol> externs = [];

    readonly Dictionary<Symbol, string> memoryAllocations = [];
    readonly Dictionary<Symbol, string> currentSsaRegisters = [];
    readonly Dictionary<Symbol, int> ssaVersionCounters = [];


    public string Output(CompilationUnit node)
    {
        memoryAllocations.Clear();
        currentSsaRegisters.Clear();
        ssaVersionCounters.Clear();

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
        foreach (Symbol external in externs)
        {
            WriteLine($"# * {external.Name}");
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
    }

    private void EndScope()
    {
        Dedent();
        WriteLine("}");
    }

    // Statements

    public void VisitFunctionStatement(FunctionStatement node)
    {
        string name = node.Symbol.Name;

        string returnType = name == "main" ? "w " : "";
        if (name == "main")
        {
            WriteLine($"export function {returnType}${name}()");
        }
        else
        {
            WriteLine($"function w ${name}()");
        }

        BeginScope();
        {
            WriteRaw("@start\n");
            foreach (Statement stmt in node.Body)
            {
                VisitStatement(stmt);
            }

            if (name == "main")
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
        foreach (Symbol extrn in node.Externals)
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
        foreach (Symbol variable in autoDeclaration.Variables)
        {
            string varName = CreateTempRegister(variable);
            Write($"{varName} =l alloc4 4");
        }
        WriteLine();
    }

    public void VisitFunctionCall(FunctionCall node)
    {
        string parameters = LoadParameterValue(node.Parameters);
        Write($"call ${node.Symbol}({parameters})");
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
                string reg = VisitStringValue(v);
                registers.Add($"l ${reg}");
                break;

                case Variable v:
                tmpReg = CreateTempRegister(v.Symbol);
                Write($"{tmpReg} =w loadw {GetRegister(v.Symbol)}");
                registers.Add($"w {tmpReg}");
                break;

                default: throw new ParserException($"Unknown parameter {expr.GetType()}({expr})");
            }
        }

        return string.Join(", ", registers);
    }

    public void VisitVariableAssignment(VariableAssignment variableAssignment)
    {
        Expression value = variableAssignment.Value;

        WriteLine("# " + value);
        string? result = VisitBinaryExpression(value);

        string reg = GetRegister(variableAssignment.Symbol);
        WriteLine($"storew {result}, {reg}");
        WriteLine();
    }

    // Expression

    public string? VisitBinaryExpression(Expression expr)
    {
        if (expr is Variable variable)
        {
            return CreateTempRegister(variable.Symbol);
        }
        else if (expr is IntValue number)
        {
            return number.Value.ToString();
        }
        else if (expr is BinaryExpression binary)
        {
            string? leftOp = VisitBinaryExpression(binary.Left);
            string? rightOp = VisitBinaryExpression(binary.Right);

            switch (binary.Operation)
            {
                case TokenType.Addition:
                Write($"add {leftOp}, {rightOp}");
                break;

                case TokenType.Multiplication:
                Write($"mul {leftOp}, {rightOp}");
                break;

                default: throw new Exception("test");
            }
        }

        return null;
    }

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

    // Utility

    public string CreateTempRegister(Symbol symbol)
    {
        if (!ssaVersionCounters.ContainsKey(symbol))
        {
            ssaVersionCounters[symbol] = 0;
        }

        ssaVersionCounters[symbol]++;

        string qbeReg = $"%{symbol.Name}_{ssaVersionCounters[symbol]}";
        currentSsaRegisters[symbol] = qbeReg;
        return qbeReg;
    }

    public string GetRegister(Symbol symbol)
    {
        if (!currentSsaRegisters.TryGetValue(symbol, out string? value))
        {
            throw new InvalidOperationException($"Variable '{symbol.Name}' (Symbol: {symbol.GetHashCode()}) has no current SSA register. Was it assigned or loaded?");
        }
        return value;
    }
}
