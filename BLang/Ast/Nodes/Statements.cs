namespace BLang.Ast.Nodes;

using System.Text.Json.Serialization;
using BLang.Ast;
using BLang.Utility;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "_Type")]
[JsonDerivedType(typeof(ExternalStatement), nameof(ExternalStatement))]
[JsonDerivedType(typeof(WhileStatement), nameof(WhileStatement))]
[JsonDerivedType(typeof(SwitchStatement), nameof(SwitchStatement))]
[JsonDerivedType(typeof(IfStatement), nameof(IfStatement))]
[JsonDerivedType(typeof(AutoStatement), nameof(AutoStatement))]
[JsonDerivedType(typeof(FunctionCall), nameof(FunctionCall))]
[JsonDerivedType(typeof(ArrayAssignmentStatement), nameof(ArrayAssignmentStatement))]
[JsonDerivedType(typeof(GlobalVariableDecleration), nameof(GlobalVariableDecleration))]
[JsonDerivedType(typeof(GlobalArrayDeclaration), nameof(GlobalArrayDeclaration))]
public abstract record Statement() : AstNode;

public record GlobalVariableDecleration(Symbol Symbol, Expression Value) : Statement
{
    public override string ToSource()
    {
        return $"{Symbol.Name} = {Value}";
    }
}

public record GlobalArrayDeclaration(Symbol Symbol, Expression[] Values, int Size) : Statement
{
    public override string ToSource()
    {
        return $"TODO:";
    }
}

public record ExternalStatement(Symbol[] Externals) : Statement
{
    public override string ToSource()
    {
        return $"TODO:";
    }
}

public record WhileStatement(BinaryExpression Condition, Statement[] Body) : Statement
{
    public override string ToSource()
    {
        return $"TODO:";
    }
}

public record SwitchStatement(BinaryExpression Condition, Statement[] Body) : Statement
{
    public override string ToSource()
    {
        return $"TODO:";
    }
}

public record IfStatement(Expression Condition, Statement[] Body, Statement[]? ElseBody) : Statement
{
    public override string ToSource()
    {
        return $"TODO:";
    }
}

public record AutoStatement(VariableAssignment[] Variables) : Statement
{
    public override string ToSource()
    {
        string vars = string.Join(", ", Variables.Select(v => v.Symbol.Name));
        return $"auto {vars};";
    }
}

public record FunctionCall(Symbol Symbol, Expression[] Parameters) : Statement
{
    public override string ToSource()
    {
        string parameters = string.Join(", ", Parameters.Select(p => p.ToSource()));
        return $"{Symbol.Name}({parameters})";
    }
}

public record VariableAssignment(Symbol Symbol, int Value);
