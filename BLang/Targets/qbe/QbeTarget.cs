namespace BLang.Targets.qbe;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BLang.Ast;
using BLang.Ast.Nodes;
using BLang.Exceptions;
using BLang.Tokenizer;
using BLang.Utility;

public class QbeTarget : ITarget
{
    public const string Target = "qbe";

    private readonly Dictionary<string, string> strings = [];
    private readonly HashSet<Symbol> externs = [];

    private readonly Dictionary<Symbol, string> currentSsaRegisters = [];
    private readonly Dictionary<string, string> memoryAllocations = [];
    private readonly Dictionary<Symbol, int> ssaVersionCounters = [];

    private int conditionIndex;
    private QbeOutput output = new();
    private CompilerContext? data;

    public Result<CompileOutput> Emit(CompilationUnit unit, CompilerContext data)
    {
        this.data = data;

        output.Clear();
        currentSsaRegisters.Clear();
        ssaVersionCounters.Clear();
        memoryAllocations.Clear();

        string file = data.File;

        (string objFile, string binFile) = GetOutputFile(file, Target);

        VisitCompilationUnit(unit);
        string qbeIR = output.Text.ToString();
        File.WriteAllText(objFile + ".ssa", qbeIR);

        StringBuilder stdError = new();
        Executable exe;

        exe = Executable.Run("qbe", $"{objFile}.ssa -o {objFile}.s").PipeErrorTo(stdError);
        if (!exe.Success) return "Failed to compile qbe ir to assembly\n" + stdError.ToString();

        exe = Executable.Run("gcc", $"{objFile}.s -o {binFile}").PipeErrorTo(stdError);
        if (!exe.Success) return "Failed to compile assembly using gcc\n" + stdError.ToString();

        return new CompileOutput(file, binFile, unit);
    }

    public void VisitCompilationUnit(CompilationUnit node)
    {
        foreach (VariableDeclaration variable in node.GlobalVariables)
        {
            output.Comment(variable.Symbol.Name);
            _ = CreateGlobalMemoryRegister(variable.Symbol);
            string value = variable.Value == null ? "w 0" : "w " + (variable.Value as IntValue)!.Value;
            output.Write($"data ${variable.Symbol.Name} = {{ {value} }}");
            output.WriteLine();
        }

        foreach (FunctionDecleration funcDecl in node.FunctionDeclarations)
        {
            VisitFunctionDecleration(funcDecl);
        }

        GenerateDataSection();
        output.WriteLine();
        GenerateExternsSection();
    }

    private void GenerateDataSection()
    {
        output.Comment("Data");
        foreach ((string value, string name) in strings)
        {
            output.Write($"data ${name} = {{ b \"{value}\", b 0 }}");
        }
    }

    private void GenerateExternsSection()
    {
        output.Comment("Externs");
        foreach (Symbol external in externs)
        {
            output.Comment($"* {external.Name}");
        }
    }

    public void VisitStatement(Statement node)
    {
        switch (node)
        {
            case ExternalStatement s: VisitExternalStatement(s); break;
            case FunctionCall s: VisitFunctionCall(s); break;
            case AutoStatement s: VisitAutoStatement(s); break;
            case VariableDeclaration s: VisitVariableAssignment(s); break;
            case WhileStatement s: VisitWhileStatement(s); break;
            case IfStatement s: VisitIfStatement(s); break;
            default: throw new ParserException(node.ToString());
        }
    }

    private void BeginScope()
    {
        output.Write("{");
        output.Indent();
    }

    private void EndScope()
    {
        output.Dedent();
        output.Write("}");
    }

    // Statements

    public void VisitFunctionDecleration(FunctionDecleration node)
    {
        string name = node.Symbol.Name;

        if (name == "main")
        {
            output.Write($"export function w ${name}()");
        }
        else
        {
            output.Write($"function w ${name}()");
        }

        BeginScope();
        output.WriteRaw("@start\n");
        EmitBody(node.Body);
        output.Write(name == "main" ? "ret 0" : "ret");
        EndScope();

        output.WriteLine();
    }

    private void EmitBody(Statement[] body)
    {
        foreach (Statement stmt in body)
        {
            VisitStatement(stmt);
        }
    }

    public void VisitExternalStatement(ExternalStatement node)
    {
        foreach (Symbol extrn in node.Externals)
        {
            _ = externs.Add(extrn);
        }
    }

    public void VisitWhileStatement(WhileStatement node)
    {
        // Comment("while " + node.Condition);
        string labelPrefix = $"@while_{++conditionIndex}_";
        output.Write($"{labelPrefix}start");
        string reg = GenerateBinaryExpressionIR(node.Condition, new("while_condition", SymbolKind.Load));
        output.Write($"jnz {reg}, {labelPrefix}body, {labelPrefix}end");
        output.Write($"{labelPrefix}body");
        output.Indent();
        {
            EmitBody(node.Body);
            output.Write($"jmp {labelPrefix}start");
        }
        output.Dedent();
        output.Write($"{labelPrefix}end");
    }

    public void VisitIfStatement(IfStatement node)
    {
        output.Comment("if " + node.Condition);
        string labelPrefix = $"@if_{++conditionIndex}_";
        output.Write($"{labelPrefix}start");
        string reg = GenerateBinaryExpressionIR(node.Condition, new("if_condition" + conditionIndex, SymbolKind.Load));
        string labelSuffix = node.ElseBody == null ? "end" : "else";
        output.Write($"jnz {reg}, {labelPrefix}body, {labelPrefix}{labelSuffix}");
        output.Write($"{labelPrefix}body");
        output.Indent();
        {
            EmitBody(node.Body);
            if (node.ElseBody != null)
            {
                output.Write($"jmp {labelPrefix}end");
                output.Write($"{labelPrefix}else");
                EmitBody(node.ElseBody);
            }
        }
        output.Dedent();
        output.Write($"{labelPrefix}end");
    }

    public void VisitAutoStatement(AutoStatement autoDeclaration)
    {
        foreach (Symbol variable in autoDeclaration.Variables)
        {
            string varName = CreateMemoryRegister(variable);
            output.Comment($"auto {variable}");
            output.Write($"{varName} =l alloc4 4");
            output.Storew(0, varName);
        }
    }

    public void VisitFunctionCall(FunctionCall node)
    {
        output.Write($"# call {node.Symbol.Name}");

        string parameters = PassParameterValue(node.Parameters);
        output.Write($"call ${node.Symbol}({parameters})");
    }

    private string PassParameterValue(Expression[] parameters)
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

                case IntValue v:
                    registers.Add($"w {v.Value}");
                    break;

                case Variable v:
                    reg = CreateTempRegister(v.Symbol);
                    output.Write($"{reg} =w loadw {GetMemoryRegister(v.Symbol)}");
                    registers.Add($"w {reg}");
                    break;

                default: throw new ParserException($"Unknown parameter {expr.GetType()}({expr})");
            }
        }

        return string.Join(", ", registers);
    }

    public void VisitVariableAssignment(VariableDeclaration variableAssignment)
    {
        Expression value = variableAssignment.Value!;// TODO: null

        output.Write($"# {variableAssignment.Symbol} = ");
        string result = GenerateBinaryExpressionIR(value, variableAssignment.Symbol);
        string reg = GetMemoryRegister(variableAssignment.Symbol);
        output.Write($"storew {result}, {reg}");
    }

    // Expression

    public string GenerateBinaryExpressionIR(Expression expr, Symbol targetSymbol)
    {
        switch (expr)
        {
            case Variable variable:
                {
                    string loadReg = CreateTempRegister(variable.Symbol);
                    string memReg = GetMemoryRegister(variable.Symbol);
                    output.Write($"{loadReg} =w loadw {memReg}");
                    return loadReg;
                }

            case IntValue number:
                {
                    return number.Value.ToString();
                }

            case AddressOfExpression address:
                {
                    if (address.Expr is not Variable var)
                        throw new ParserException("Address-of operator only supported for variables.");

                    return GetMemoryRegister(var.Symbol);
                }

            case PointerDereferenceExpression pointerDereference:
                {
                    if (pointerDereference.Expr is not Variable variable)
                        throw new ParserException("Pointer dereference operator only supported for variables.");

                    string addressReg = CreateTempRegister(variable.Symbol);
                    string memReg = GetMemoryRegister(variable.Symbol);
                    output.Write($"{addressReg} =l loadw {memReg}");

                    output.Write($"# test");
                    string loadReg = NewTempReg();
                    output.Write($"{loadReg} =w loadw {addressReg}");
                    return loadReg;
                }

            case BinaryExpression binary:
                {
                    string leftOp = GenerateBinaryExpressionIR(binary.Left, targetSymbol);
                    string rightOp = GenerateBinaryExpressionIR(binary.Right, targetSymbol);

                    string reg = CreateTempRegister(targetSymbol);
#pragma warning disable IDE0010
                    switch (binary.Operation)
#pragma warning restore IDE0010
                    {
                        case TokenType.Addition:
                            output.Write($"{reg} =w add {leftOp}, {rightOp}");
                            break;

                        case TokenType.Multiplication:
                            output.Write($"{reg} =w mul {leftOp}, {rightOp}");
                            break;

                        case TokenType.LessThan:
                            output.Write($"{reg} =w csltw {leftOp}, {rightOp}");
                            break;

                        case TokenType.LessThanEqual:
                            output.Write($"{reg} =w cslew {leftOp}, {rightOp}");
                            break;

                        case TokenType.Modulo:
                            output.Write($"{reg} =w rem {leftOp}, {rightOp}");
                            break;

                        case TokenType.EqualEqual:
                            output.Write($"{reg} =w ceqw {leftOp}, {rightOp}");
                            break;

                        case TokenType.BitwiseOr:
                            output.Write($"{reg} =w or {leftOp}, {rightOp}");
                            break;

                        default: throw new ParserException("Unknown operator " + binary.Operation);
                    }
                    return reg;
                }

            default: throw new ParserException("Unknown expression type " + expr);
        }

        throw new ParserException("Unreachable");
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
        ssaVersionCounters[symbol] = !ssaVersionCounters.TryGetValue(symbol, out int value) ? 0 : ++value;

        string qbeReg = $"%{symbol.Name}_{ssaVersionCounters[symbol]}";
        currentSsaRegisters[symbol] = qbeReg;
        return qbeReg;
    }

    private int tempRegister;
    public string NewTempReg()
    {
        string qbeReg = $"%temp_{tempRegister}";
        tempRegister++;
        return qbeReg;
    }

    public string GetRegister(Symbol symbol)
    {
        if (currentSsaRegisters.TryGetValue(symbol, out string? value))
            return value;
        throw new InvalidOperationException($"Variable '{symbol.Name}' has no current SSA register.");
    }

    public string CreateMemoryRegister(Symbol symbol)
    {
        if (memoryAllocations.ContainsKey(symbol.Name))
            throw new InvalidOperationException($"Variable '{symbol.Name}' is already registered.");

        string qbeReg = $"%{symbol.Name}_ptr";
        memoryAllocations[symbol.Name] = qbeReg;
        return qbeReg;
    }

    public string CreateGlobalMemoryRegister(Symbol symbol)
    {
        if (memoryAllocations.ContainsKey(symbol.Name))
            throw new InvalidOperationException($"Variable '{symbol.Name}' is already registered.");

        string qbeReg = $"${symbol.Name}";
        memoryAllocations.Add(symbol.Name, qbeReg);
        return qbeReg;
    }


    public string GetMemoryRegister(Symbol symbol)
    {
        if (!memoryAllocations.TryGetValue(symbol.Name, out string? value))
            throw new InvalidOperationException($"Variable '{symbol.Name}' has no current SSA register.");

        return value;
    }

    private static (string, string) GetOutputFile(string inputFile, string target)
    {
        CreateOutputDirectories(inputFile, Target);

        string projectDirectory = Path.GetDirectoryName(inputFile)!;
        string sourceFileName = Path.GetFileNameWithoutExtension(inputFile);
        string objFile = Path.Combine(projectDirectory, "obj", target, sourceFileName);
        string binFile = Path.Combine(projectDirectory, "bin", target, sourceFileName);

        return (objFile, binFile);
    }

    private static void CreateOutputDirectories(string inputFile, string target)
    {
        string projectDirectory = Path.GetDirectoryName(inputFile)!;

        _ = Directory.CreateDirectory(Path.Combine(projectDirectory, "obj", target));
        _ = Directory.CreateDirectory(Path.Combine(projectDirectory, "bin", target));
    }
}
