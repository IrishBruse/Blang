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

    private SymbolTable symbolTable = new();

    private int conditionIndex;
    private QbeOutput qbe = new();

    public Result<EmitOutput> Emit(CompilationUnit unit, CompilerContext data)
    {
        symbolTable = data.Symbols;

        symbolTable.Clear();
        qbe.Clear();

        string file = data.File;

        (string objFile, string binFile) = GetOutputFile(file, Target);

        string error = "";

        try
        {
            VisitCompilationUnit(unit);
        }
        catch (Exception e)
        {
            error = Options.Verbose > 0 ? e.ToString() : e.Message;
        }
        string qbeIR = qbe.Text.ToString();
        File.WriteAllText(objFile + ".ssa", qbeIR);

        if (error != "")
        {
            return error;
        }

        StringBuilder stdError = new();
        Executable exe;

        exe = Executable.Run("qbe", $"{objFile}.ssa -o {objFile}.s").PipeErrorTo(stdError);
        if (!exe.Success) return $"Failed to compile \"{objFile}.ssa\" to assembly\n" + stdError.ToString().Replace("qbe:", "");

        exe = Executable.Run("gcc", $"{objFile}.s -o {binFile}").PipeErrorTo(stdError);
        if (!exe.Success) return $"Failed to compile \"{objFile}.ssa\" into valid \"{objFile}.s\"\n" + stdError.ToString().Replace("qbe:", "");

        return new EmitOutput(binFile, unit);
    }

    public void VisitCompilationUnit(CompilationUnit node)
    {
        foreach (GlobalVariable variable in node.GlobalVariables)
        {
            switch (variable)
            {
                case GlobalVariableDecleration varDecl:
                    VisitGlobalVariable(varDecl);
                    break;

                case GlobalArrayDeclaration arrayDecl:
                    VisitGlobalArray(arrayDecl);
                    break;

                default: break;
            }
        }

        foreach (FunctionDecleration funcDecl in node.FunctionDeclarations)
        {
            VisitFunctionDecleration(funcDecl);
        }

        GenerateDataSection();
        qbe.WriteLine();
        GenerateExternsSection();
    }

    private void VisitGlobalVariable(GlobalVariableDecleration variable)
    {
        qbe.Comment(variable.Symbol.Name);
        string value = variable.Value == null ? "l 0" : "l " + (variable.Value as IntValue)!.Value;
        qbe.Data(variable.Symbol.Name, value);
        qbe.WriteLine();
    }

    private void VisitGlobalArray(GlobalArrayDeclaration array)
    {
        qbe.Comment($"{array.Symbol.Name}[{array.Size}]");
        Expression[] values = array.Values;
        string data = "w";
        for (int i = 0; i < array.Size; i++)
        {
            data += " " + (i < values.Length ? values[i] : 0);
        }

        qbe.Data(array.Symbol.Name, data);
        qbe.AllocateMemoryReg(array.Symbol, "$" + array.Symbol.Name);

        qbe.WriteLine();
    }

    private void VisitArrayAssignment(ArrayAssignmentStatement array)
    {
        qbe.Comment($"{array.Symbol}[{array.Index}] = {array.Value}");
        string addrReg = qbe.GetMemoryRegister(array.Symbol, false);

        string idxReg = VisitExpression(array.Index);
        string temp = qbe.Extuw(idxReg);
        addrReg = qbe.Add(addrReg, temp, Size.L);

        string valueReg = VisitExpression(array.Value);

        qbe.Storel(valueReg, addrReg);
        qbe.WriteLine();
    }

    public void VisitVariableAssignment(GlobalVariableDecleration variableAssignment)
    {
        qbe.SetRegisterName(variableAssignment.Symbol);

        Expression value = variableAssignment.Value;
        qbe.Comment($"{variableAssignment.Symbol} = {value}");

        string result = VisitExpression(value);
        string memoryAddressReg = qbe.GetMemoryAddress(variableAssignment.Symbol);
        qbe.Storel(result, memoryAddressReg);
    }

    private void GenerateDataSection()
    {
        qbe.Comment("Data");
        foreach ((string value, string name) in strings)
        {
            qbe.DataString(name, value);
        }
    }

    private void GenerateExternsSection()
    {
        qbe.Comment("Externs");
        foreach (Symbol external in externs)
        {
            qbe.Comment($"- {external.Name}");
        }
    }

    public void VisitStatement(Statement node)
    {
        switch (node)
        {
            case AutoStatement s: VisitAutoStatement(s); break;
            case ExternalStatement s: VisitExternalStatement(s); break;
            // TODO: Case
            case IfStatement s: VisitIfStatement(s); break;
            case WhileStatement s: VisitWhileStatement(s); break;
            case GlobalVariableDecleration s: VisitVariableAssignment(s); break;
            // TODO: Goto
            // TODO: Label
            case FunctionCall s: VisitFunctionCall(s); break;

            case ArrayAssignmentStatement s: VisitArrayAssignment(s); break;
            default: throw new ParserException(node.ToString());
        }
    }

    private void BeginScope()
    {
        qbe.BeginScope();
    }

    private void EndScope()
    {
        qbe.EndScope();
    }

    // Statements

    public void VisitFunctionDecleration(FunctionDecleration node)
    {
        string name = node.Symbol.Name;

        if (name == "main")
        {
            qbe.ExportFunction(name);
        }
        else
        {
            string[] parameters = node.Parameters.Select(v =>
            {
                _ = qbe.GetMemoryRegister(v.Symbol, true);
                string reg = v.Symbol.Name;
                return $"l %{reg}_0";
            }).ToArray();
            qbe.Function(name, "w", parameters);
        }

        BeginScope();
        {
            qbe.Label("start");
            EmitBody(node.Body);
            qbe.Ret(name == "main" ? 0 : null);
        }
        EndScope();

        qbe.ClearMemoryRegisters();

        qbe.WriteLine();
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
        qbe.Comment("while " + node.Condition);
        string labelPrefix = $"while_{++conditionIndex}_";
        qbe.Label($"{labelPrefix}condition");
        string reg = VisitExpression(node.Condition);
        qbe.Jnz(reg, $"{labelPrefix}body", $"{labelPrefix}end");
        qbe.Label($"{labelPrefix}body");
        EmitBody(node.Body);
        qbe.Jmp($"{labelPrefix}condition");
        qbe.Label($"{labelPrefix}end");
    }

    public void VisitIfStatement(IfStatement node)
    {
        qbe.Comment("if " + node.Condition);
        qbe.Indent();

        // TODO: refactor incrementing into qbe.Label
        string labelPrefix = $"if_{++conditionIndex}_";
        qbe.Label($"{labelPrefix}condition");

        string reg = VisitExpression(node.Condition);
        string labelSuffix = node.ElseBody == null ? "end" : "else";
        qbe.Jnz(reg, $"{labelPrefix}body", $"{labelPrefix}{labelSuffix}");
        qbe.Label($"{labelPrefix}body");

        EmitBody(node.Body);

        if (node.ElseBody != null)
        {
            qbe.Jmp(labelPrefix + "end");
            qbe.Label($"{labelPrefix}else");

            EmitBody(node.ElseBody);
        }

        qbe.Label($"{labelPrefix}end");
        qbe.Unindent();
    }

    public void VisitAutoStatement(AutoStatement autoDeclaration)
    {
        foreach (VariableAssignment variable in autoDeclaration.Variables)
        {
            qbe.Comment($"auto {variable.Symbol} = {variable.Value}");
            string reg = qbe.Alloc8(8, Size.L);
            qbe.AllocateMemoryReg(variable.Symbol, reg);
            qbe.Storew(variable.Value, reg);
        }
    }

    public void VisitFunctionCall(FunctionCall node)
    {
        qbe.Comment("call " + node.Symbol.Name);

        string parameters = PassParameterValue(node.Parameters);
        qbe.Call(node.Symbol.ToString(), parameters);
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
                    registers.Add($"l {v.Value}");
                    break;

                case Variable v:
                    qbe.SetRegisterName(v.Symbol);

                    reg = qbe.Loadl(qbe.GetMemoryAddress(v.Symbol));
                    registers.Add($"l {reg}");
                    break;

                case ArrayIndexExpression array:
                    qbe.Comment(array.ToString());
                    string memReg = qbe.GetMemoryAddress(array.Variable.Symbol);
                    string arg = qbe.Loadl(memReg, Size.L);
                    if (array.Index is IntValue i && i.Value != 0)
                    {
                        arg = qbe.Add(arg, array.Index.ToString());
                    }
                    registers.Add($"l {arg}");
                    break;

                case BinaryExpression expression:
                    qbe.Comment(expression.ToString());
                    string exprReg = VisitBinaryExpression(expression);
                    registers.Add("l " + exprReg);
                    break;

                default: throw new ParserException($"Unknown parameter {expr.GetType()}({expr})");
            }
        }

        return string.Join(", ", registers.ToArray());
    }

    // Expression

    public string VisitExpression(Expression expr)
    {
        return expr switch
        {
            Variable variable => VisitVariable(variable),
            IntValue number => number.Value.ToString(),
            AddressOfExpression address => GenerateAddressOfExpressionIR(address),
            PointerDereferenceExpression pointer => GeneratePointerDereferenceExpressionIR(pointer),
            ArrayIndexExpression array => GenerateArrayIndexExpressionIR(array),
            BinaryExpression binary => VisitBinaryExpression(binary),
            _ => throw new ParserException($"Unknown expression type {expr.GetType()}: {expr}"),
        };
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

    private string VisitVariable(Variable variable)
    {
        string memoryAddressReg = qbe.GetMemoryAddress(variable.Symbol);
        return qbe.Loadl(memoryAddressReg);
    }

    private string GenerateAddressOfExpressionIR(AddressOfExpression address)
    {

        if (address.Expr is not Variable var)
        {
            throw new ParserException("Address-of operator only supported for variables.");
        }

        return qbe.GetMemoryRegister(var.Symbol);
    }

    private string GeneratePointerDereferenceExpressionIR(PointerDereferenceExpression pointer)
    {

        if (pointer.Expr is not Variable variable)
        {
            throw new ParserException("Pointer dereference operator only supported for variables.");
        }

        string memReg = qbe.GetMemoryRegister(variable.Symbol);
        qbe.Comment("Load Pointer");
        string addressReg = qbe.Loadl(memReg, Size.L);
        qbe.Comment("Dereferece Pointer");
        return qbe.Loadl(addressReg);
    }

    private string GenerateArrayIndexExpressionIR(ArrayIndexExpression array)
    {
        Variable variable = array.Variable;

        Expression index = array.Index;
        string indexReg = VisitExpression(index);

        string memReg = qbe.GetMemoryAddress(variable.Symbol);
        qbe.Comment("TODO: Load Array " + indexReg);
        string addressReg = qbe.Loadl(memReg, Size.L);
        qbe.Comment("TODO: Dereferece Array");
        return qbe.Loadl(addressReg);
    }

    private string VisitBinaryExpression(BinaryExpression binary)
    {
        string leftOp = VisitExpression(binary.Left);
        string rightOp = VisitExpression(binary.Right);

        BinaryOperator binOperator = binary.Operation;
        qbe.Comment(binOperator.ToText());
        return binOperator switch
        {
            BinaryOperator.Addition => qbe.Add(leftOp, rightOp),
            BinaryOperator.Subtraction => qbe.Sub(leftOp, rightOp),
            BinaryOperator.BitwiseOr => qbe.Or(leftOp, rightOp),
            BinaryOperator.EqualEqual => qbe.Ceql(leftOp, rightOp),
            BinaryOperator.LessThan => qbe.Csltl(leftOp, rightOp),
            BinaryOperator.LessThanEqual => qbe.Cslel(leftOp, rightOp),
            BinaryOperator.Modulo => qbe.Rem(leftOp, rightOp),
            BinaryOperator.Multiplication => qbe.Mul(leftOp, rightOp),
            BinaryOperator.BitwiseShiftLeft => "%Error",
            BinaryOperator.BitwiseShiftRight => "%Error",
            BinaryOperator.BitwiseAnd => "%Error",

            _ => throw new ParserException("Unknown operator " + binary.Operation),
        };
    }

    // Utility

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
