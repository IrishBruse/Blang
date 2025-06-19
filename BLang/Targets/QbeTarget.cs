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
            string varName = CreateMemoryRegister(variable);
            Write($"{varName} =l alloc4 4");
        }
        WriteLine();
    }

    public void VisitFunctionCall(FunctionCall node)
    {
        WriteLine($"# call {node.Symbol.Name}");

        string parameters = LoadParameterValue(node.Parameters);
        Write($"call ${node.Symbol}({parameters})");
        WriteLine();
    }

    string LoadParameterValue(Expression[] parameters)
    {
        List<string> registers = [];
        foreach (AstNode expr in parameters)
        {
            string reg;
            switch (expr)
            {
                case StringValue v:
                reg = VisitStringValue(v);
                registers.Add($"l ${reg}");
                break;

                case Variable v:
                reg = CreateTempRegister(v.Symbol);
                Write($"{reg} =w loadw {GetMemoryRegister(v.Symbol)}");
                registers.Add($"w {reg}");
                break;

                default: throw new ParserException($"Unknown parameter {expr.GetType()}({expr})");
            }
        }

        return string.Join(", ", registers);
    }

    public void VisitVariableAssignment(VariableAssignment variableAssignment)
    {
        Expression value = variableAssignment.Value;

        WriteLine($"# {variableAssignment.Symbol} = {value}");
        string? result = GenerateBinaryExpressionIR(value, variableAssignment.Symbol);
        string reg = GetMemoryRegister(variableAssignment.Symbol);
        WriteLine($"storew {result}, {reg}");
        WriteLine();
    }

    // Expression

    public string? GenerateBinaryExpressionIR(Expression expr, Symbol targetSymbol)
    {
        switch (expr)
        {
            case Variable variable:
            {
                string loadReg = CreateTempRegister(variable.Symbol);
                string memReg = GetMemoryRegister(variable.Symbol);
                Write($"{loadReg} =w loadw {memReg}");
                return loadReg;
            }

            case IntValue number:
            {
                return number.Value.ToString();
            }

            case BinaryExpression binary:
            {
                string? leftOp = GenerateBinaryExpressionIR(binary.Left, targetSymbol);
                string? rightOp = GenerateBinaryExpressionIR(binary.Right, targetSymbol);

                string reg = CreateTempRegister(targetSymbol);
                switch (binary.Operation)
                {
                    case TokenType.Addition:
                    Write($"{reg} =w add {leftOp}, {rightOp}");
                    break;

                    case TokenType.Multiplication:
                    Write($"{reg} =w mul {leftOp}, {rightOp}");
                    break;

                    default: throw new Exception("test");
                }
                return reg;
            }
        }

        throw new Exception("Unreachable");
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
        if (!ssaVersionCounters.TryGetValue(symbol, out int value))
        {
            ssaVersionCounters[symbol] = 0;
        }
        else
        {
            ssaVersionCounters[symbol] = ++value;
        }

        string qbeReg = $"%{symbol.Name}_{ssaVersionCounters[symbol]}";
        currentSsaRegisters[symbol] = qbeReg;
        return qbeReg;
    }

    public string GetRegister(Symbol symbol)
    {
        if (currentSsaRegisters.TryGetValue(symbol, out string? value))
        {
            return value;
        }
        throw new InvalidOperationException($"Variable '{symbol.Name}' has no current SSA register.");
    }

    public string CreateMemoryRegister(Symbol symbol)
    {
        if (memoryAllocations.ContainsKey(symbol))
        {
            throw new InvalidOperationException($"Variable '{symbol.Name}' is already registered.");
        }

        string qbeReg = $"%{symbol.Name}_ptr";
        memoryAllocations[symbol] = qbeReg;
        return qbeReg;
    }

    public string GetMemoryRegister(Symbol symbol)
    {
        if (memoryAllocations.TryGetValue(symbol, out string? value))
        {
            return value;
        }
        throw new InvalidOperationException($"Variable '{symbol.Name}' has no current SSA register.");
    }
}
