namespace BLang.Ast.Nodes;

using System.Text.Json.Serialization;
using BLang.Utility;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "_Type")]
[JsonDerivedType(typeof(VariableDecleration), nameof(VariableDecleration))]
[JsonDerivedType(typeof(ExternalStatement), nameof(ExternalStatement))]
[JsonDerivedType(typeof(WhileStatement), nameof(WhileStatement))]
[JsonDerivedType(typeof(SwitchStatement), nameof(SwitchStatement))]
[JsonDerivedType(typeof(IfStatement), nameof(IfStatement))]
[JsonDerivedType(typeof(AutoStatement), nameof(AutoStatement))]
[JsonDerivedType(typeof(FunctionCall), nameof(FunctionCall))]
[JsonDerivedType(typeof(VariableDeclaration), nameof(VariableDeclaration))]
public abstract record Statement() : AstNode;

public record VariableDecleration(Symbol Symbol, int? Value) : Statement;

public record ExternalStatement(Symbol[] Externals) : Statement;

public record WhileStatement(BinaryExpression Condition, Statement[] Body) : Statement;

public record SwitchStatement(BinaryExpression Condition, Statement[] Body) : Statement;

public record IfStatement(BinaryExpression Condition, Statement[] Body, Statement[]? ElseBody) : Statement;

public record AutoStatement(Symbol[] Variables) : Statement;

public record FunctionCall(Symbol Symbol, Expression[] Parameters) : Statement;

public record VariableDeclaration(Symbol Symbol, Expression Value) : Statement;
