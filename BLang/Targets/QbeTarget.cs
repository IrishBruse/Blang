namespace BLang.Targets;

using System;
using System.Collections.Generic;

using BLang.Ast.Nodes;
using BLang.Exceptions;
using BLang.Tokenizer;
using BLang.Utility;

public class QbeTarget(CompilationData data) : BaseTarget
{
    public const string Target = "qbe";
    public override int Indention => 4;

    readonly Dictionary<string, string> strings = [];
    readonly HashSet<Symbol> externs = [];

    readonly Dictionary<Symbol, string> currentSsaRegisters = [];
    readonly Dictionary<string, string> memoryAllocations = [];
    readonly Dictionary<Symbol, int> ssaVersionCounters = [];

    int conditionIndex = 0;

    public string Output(CompilationUnit node)
    {
        currentSsaRegisters.Clear();
        ssaVersionCounters.Clear();
        memoryAllocations.Clear();

        _ = data;

        VisitCompilationUnit(node);
        return output.ToString();
    }

    public void VisitCompilationUnit(CompilationUnit node)
    {
        foreach (GlobalVariable variable in node.GlobalVariables)
        {
            Comment(variable.Symbol.Name);
            CreateGlobalMemoryRegister(variable.Symbol);
            string value = variable.Value == null ? "z 4" : "w " + variable.Value;
            Write($"data ${variable.Symbol.Name} = {{ {value} }}");
            Write();
        }

        foreach (FunctionDecleration funcDecl in node.FunctionDeclarations)
        {
            VisitFunctionStatement(funcDecl);
        }

        GenerateDataSection();
        Write();
        GenerateExternsSection();
    }

    void GenerateDataSection()
    {
        Comment("Data");
        foreach ((string value, string name) in strings)
        {
            Write($"data ${name} = {{ b \"{value}\", b 0 }}");
        }
    }

    void GenerateExternsSection()
    {
        Comment("Externs");
        foreach (Symbol external in externs)
        {
            Comment($"* {external.Name}");
        }
    }

    public void VisitStatement(Statement node)
    {
        switch (node)
        {
            case ExternalStatement s: VisitExternalStatement(s); break;
            case FunctionCall s: VisitFunctionCall(s); break;
            case AutoStatement s: VisitAutoStatement(s); break;
            case VariableDeclarator s: VisitVariableAssignment(s); break;
            case WhileStatement s: VisitWhileStatement(s); break;
            case IfStatement s: VisitIfStatement(s); break;
            default: throw new Exception(node.ToString());
        }
    }

    void BeginScope()
    {
        Write("{");
        Indent();
    }

    void EndScope()
    {
        Dedent();
        Write("}");
    }

    // Statements

    public void VisitFunctionStatement(FunctionDecleration node)
    {
        string name = node.Symbol.Name;

        string returnType = name == "main" ? "w " : "";
        if (name == "main")
        {
            Write($"export function {returnType}${name}()");
        }
        else
        {
            Write($"function w ${name}()");
        }

        BeginScope();
        WriteRaw("@start\n");
        EmitBody(node.Body);
        Write(name == "main" ? "ret 0" : "ret");
        EndScope();

        Write();
    }

    void EmitBody(Statement[] body)
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
            externs.Add(extrn);
        }
    }

    public void VisitWhileStatement(WhileStatement node)
    {
        Comment("while " + node.Condition);
        string labelPrefix = $"@while_{++conditionIndex}_";
        Write($"{labelPrefix}start");
        string reg = GenerateBinaryExpressionIR(node.Condition, new("while_condition", SymbolKind.Load));
        Write($"jnz {reg}, {labelPrefix}body, {labelPrefix}end");
        Write($"{labelPrefix}body");
        Indent();
        {
            EmitBody(node.Body);
            Write($"jmp {labelPrefix}start");
        }
        Dedent();
        Write($"{labelPrefix}end");
    }

    public void VisitIfStatement(IfStatement node)
    {
        Comment("if " + node.Condition);
        string labelPrefix = $"@if_{++conditionIndex}_";
        Write($"{labelPrefix}start");
        string reg = GenerateBinaryExpressionIR(node.Condition, new("if_condition" + conditionIndex, SymbolKind.Load));
        string labelSuffix = node.ElseBody == null ? "end" : "else";
        Write($"jnz {reg}, {labelPrefix}body, {labelPrefix}{labelSuffix}");
        Write($"{labelPrefix}body");
        Indent();
        {
            EmitBody(node.Body);
            if (node.ElseBody != null)
            {
                Write($"jmp {labelPrefix}end");
                Write($"{labelPrefix}else");
                EmitBody(node.ElseBody);
            }
        }
        Dedent();
        Write($"{labelPrefix}end");
    }

    public void VisitAutoStatement(AutoStatement autoDeclaration)
    {
        foreach (Symbol variable in autoDeclaration.Variables)
        {
            string varName = CreateMemoryRegister(variable);
            Write($"# auto {variable}");
            Write($"{varName} =l alloc4 4");
            Write($"storew 0, {varName}");
        }
    }

    public void VisitFunctionCall(FunctionCall node)
    {
        Write($"# call {node.Symbol.Name}");

        string parameters = PassParameterValue(node.Parameters);
        Write($"call ${node.Symbol}({parameters})");
    }

    string PassParameterValue(Expression[] parameters)
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
                Write($"{reg} =w loadw {GetMemoryRegister(v.Symbol)}");
                registers.Add($"w {reg}");
                break;

                default: throw new ParserException($"Unknown parameter {expr.GetType()}({expr})");
            }
        }

        return string.Join(", ", registers);
    }

    public void VisitVariableAssignment(VariableDeclarator variableAssignment)
    {
        Expression value = variableAssignment.Value;

        Write($"# {variableAssignment.Symbol} = {value}");
        string result = GenerateBinaryExpressionIR(value, variableAssignment.Symbol);
        string reg = GetMemoryRegister(variableAssignment.Symbol);
        Write($"storew {result}, {reg}");
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
                Write($"{loadReg} =w loadw {memReg}");
                return loadReg;
            }

            case IntValue number:
            {
                return number.Value.ToString();
            }

            case AddressOfExpression address:
            {
                if (address.expr is not Variable var)
                {
                    throw new Exception("Address-of operator only supported for variables.");
                }

                return GetMemoryRegister(var.Symbol);
            }

            case PointerDereferenceExpression pointerDereference:
            {
                if (pointerDereference.expr is not Variable variable)
                {
                    throw new Exception("Pointer dereference operator only supported for variables.");
                }

                string addressReg = CreateTempRegister(variable.Symbol);
                string memReg = GetMemoryRegister(variable.Symbol);
                Write($"{addressReg} =l loadw {memReg}");

                Write($"# test");
                string loadReg = NewTempReg();
                Write($"{loadReg} =w loadw {addressReg}");
                return loadReg;
            }

            case BinaryExpression binary:
            {
                string leftOp = GenerateBinaryExpressionIR(binary.Left, targetSymbol);
                string rightOp = GenerateBinaryExpressionIR(binary.Right, targetSymbol);

                string reg = CreateTempRegister(targetSymbol);
                switch (binary.Operation)
                {
                    case TokenType.Addition:
                    Write($"{reg} =w add {leftOp}, {rightOp}");
                    break;

                    case TokenType.Multiplication:
                    Write($"{reg} =w mul {leftOp}, {rightOp}");
                    break;

                    case TokenType.LessThan:
                    Write($"{reg} =w csltw {leftOp}, {rightOp}");
                    break;

                    case TokenType.LessThanEqual:
                    Write($"{reg} =w cslew {leftOp}, {rightOp}");
                    break;

                    case TokenType.Modulo:
                    Write($"{reg} =w rem {leftOp}, {rightOp}");
                    break;

                    case TokenType.EqualEqual:
                    Write($"{reg} =w ceqw {leftOp}, {rightOp}");
                    break;

                    case TokenType.BitwiseOr:
                    Write($"{reg} =w or {leftOp}, {rightOp}");
                    break;

                    default: throw new Exception("Unknown operator " + binary.Operation);
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

    int tempRegister = 0;
    public string NewTempReg()
    {
        string qbeReg = $"%temp_{tempRegister}";
        tempRegister++;
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
        if (memoryAllocations.ContainsKey(symbol.Name))
        {
            throw new InvalidOperationException($"Variable '{symbol.Name}' is already registered.");
        }

        string qbeReg = $"%{symbol.Name}_ptr";
        memoryAllocations[symbol.Name] = qbeReg;
        return qbeReg;
    }

    public string CreateGlobalMemoryRegister(Symbol symbol)
    {
        if (memoryAllocations.ContainsKey(symbol.Name))
        {
            throw new InvalidOperationException($"Variable '{symbol.Name}' is already registered.");
        }

        string qbeReg = $"${symbol.Name}";
        memoryAllocations.Add(symbol.Name, qbeReg);
        return qbeReg;
    }


    public string GetMemoryRegister(Symbol symbol)
    {
        if (!memoryAllocations.TryGetValue(symbol.Name, out string? value))
        {
            throw new InvalidOperationException($"Variable '{symbol.Name}' has no current SSA register.");
        }

        return value;
    }

    public void Comment(string message)
    {
        Write($"# {message}");
    }
}
