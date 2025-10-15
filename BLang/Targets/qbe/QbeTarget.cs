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

    private readonly Dictionary<string, string> memoryAllocations = [];

    private int conditionIndex;
    private QbeOutput qbe = new();

    public Result<CompileOutput> Emit(CompilationUnit unit, CompilerContext data)
    {
        _ = data;

        qbe.Clear();
        memoryAllocations.Clear();

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

        return new CompileOutput(file, binFile, unit);
    }

    // Program (CompilationUnit)
    //     Definition*
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
        _ = CreateGlobalMemoryRegister(variable.Symbol);// TODO: remove
        string value = variable.Value == null ? "w 0" : "w " + (variable.Value as IntValue)!.Value;
        qbe.Data(variable.Symbol.Name, value);
        qbe.WriteLine();
    }

    private void VisitGlobalArray(GlobalArrayDeclaration array)
    {
        qbe.Comment($"{array.Symbol.Name}[{array.Size}]");
        _ = CreateGlobalMemoryRegister(array.Symbol);// TODO: remove
        int[] values = array.Values;
        string data = "w";
        for (int i = 0; i < array.Size; i++)
        {
            data += " " + (i < values.Length ? values[i] : 0);
        }
        qbe.Data(array.Symbol.Name, data);
        qbe.WriteLine();
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
            case ExternalStatement s: VisitExternalStatement(s); break;
            case FunctionCall s: VisitFunctionCall(s); break;
            case AutoStatement s: VisitAutoStatement(s); break;
            case GlobalVariableDecleration s: VisitVariableAssignment(s); break;
            case WhileStatement s: VisitWhileStatement(s); break;
            case IfStatement s: VisitIfStatement(s); break;
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
            qbe.Function(name);
        }

        BeginScope();
        {
            qbe.Label("start");
            EmitBody(node.Body);
            qbe.Ret(name == "main" ? 0 : null);
        }
        EndScope();

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
        string reg = GenerateBinaryExpressionIR(node.Condition, new("while_condition"));
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

        string reg = GenerateBinaryExpressionIR(node.Condition, new("if_condition" + conditionIndex));
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
            qbe.Comment($"auto {variable}");
            string varName = qbe.Alloc4(4, Size.L);
            memoryAllocations[variable.Symbol.Name] = varName;// TODO:
            qbe.Storew(variable.Value, varName);
        }
    }

    public void VisitFunctionCall(FunctionCall node)
    {
        qbe.Comment("call", node.Symbol.Name);

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
                    registers.Add($"w {v.Value}");
                    break;

                case Variable v:
                    reg = qbe.Loadw(GetMemoryRegister(v.Symbol));
                    registers.Add($"w {reg}");
                    break;

                case ArrayIndexExpression array:
                    qbe.Comment(array.ToString());

                    string memReg = GetMemoryRegister(array.Variable.Symbol);
                    string arg = qbe.Loadw(memReg, Size.L);
                    if (array.Index is IntValue i && i.Value != 0)
                    {
                        arg = qbe.Add(arg, array.Index.ToString());
                    }
                    registers.Add($"w {arg}");
                    break;

                default: throw new ParserException($"Unknown parameter {expr.GetType()}({expr})");
            }
        }

        return string.Join(", ", registers);
    }

    public void VisitVariableAssignment(GlobalVariableDecleration variableAssignment)
    {
        Expression value = variableAssignment.Value!;// TODO: null

        qbe.Comment($"{variableAssignment.Symbol} = {value}");
        string result = GenerateBinaryExpressionIR(value, variableAssignment.Symbol);
        string reg = GetMemoryRegister(variableAssignment.Symbol);
        qbe.Storew(result, reg);
    }

    // Expression

    public string GenerateBinaryExpressionIR(Expression expr, Symbol targetSymbol)
    {
        switch (expr)
        {
            case Variable variable:
                {
                    string memReg = GetMemoryRegister(variable.Symbol);
                    return qbe.Loadw(memReg);
                }

            case IntValue number:
                {
                    return number.Value.ToString();
                }

            case AddressOfExpression address:
                {
                    if (address.Expr is not Variable var)
                    {
                        throw new ParserException("Address-of operator only supported for variables.");
                    }

                    return GetMemoryRegister(var.Symbol);
                }

            case PointerDereferenceExpression pointer:
                {
                    if (pointer.Expr is not Variable variable)
                    {
                        throw new ParserException("Pointer dereference operator only supported for variables.");
                    }

                    qbe.SetTempRegName(variable.Symbol);
                    // TODO: encode above into the return type below to be passed along
                    string memReg = GetMemoryRegister(variable.Symbol);
                    string addressReg = qbe.Loadw(memReg, Size.L);
                    return qbe.Loadw(addressReg);
                }

            case BinaryExpression binary:
                {
                    string leftOp = GenerateBinaryExpressionIR(binary.Left, targetSymbol);
                    string rightOp = GenerateBinaryExpressionIR(binary.Right, targetSymbol);

                    BinaryOperator binOperator = binary.Operation;
                    return binOperator switch
                    {
                        BinaryOperator.Addition => qbe.Add(leftOp, rightOp),
                        BinaryOperator.BitwiseOr => qbe.Or(leftOp, rightOp),
                        BinaryOperator.EqualEqual => qbe.Ceqw(leftOp, rightOp),
                        BinaryOperator.LessThan => qbe.Csltw(leftOp, rightOp),
                        BinaryOperator.LessThanEqual => qbe.Cslew(leftOp, rightOp),
                        BinaryOperator.Modulo => qbe.Rem(leftOp, rightOp),
                        BinaryOperator.Multiplication => qbe.Mul(leftOp, rightOp),

                        BinaryOperator.Subtraction => throw new NotImplementedException(),
                        BinaryOperator.Division => throw new NotImplementedException(),
                        BinaryOperator.GreaterThan => throw new NotImplementedException(),
                        BinaryOperator.GreaterThanEqual => throw new NotImplementedException(),
                        BinaryOperator.NotEqual => throw new NotImplementedException(),
                        BinaryOperator.LogicalAnd => throw new NotImplementedException(),
                        BinaryOperator.LogicalOr => throw new NotImplementedException(),
                        BinaryOperator.LogicalNot => throw new NotImplementedException(),
                        BinaryOperator.Increment => throw new NotImplementedException(),
                        BinaryOperator.Decrement => throw new NotImplementedException(),
                        BinaryOperator.BitwiseComplement => throw new NotImplementedException(),
                        BinaryOperator.BitwiseAnd => throw new NotImplementedException(),
                        BinaryOperator.BitwiseXOr => throw new NotImplementedException(),
                        BinaryOperator.BitwiseShiftLeft => throw new NotImplementedException(),
                        BinaryOperator.BitwiseShiftRight => throw new NotImplementedException(),

                        BinaryOperator.None => throw new NotImplementedException(),
                        BinaryOperator.ArrayIndexing => throw new NotImplementedException(),

                        _ => throw new ParserException("Unknown operator " + binary.Operation),
                    };
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
