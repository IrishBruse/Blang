namespace BLang.Ast.Nodes;

using System.Text.Json.Serialization;
using BLang.Ast;
using BLang.Utility;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "_Type")]
[JsonDerivedType(typeof(VariableDeclaration), nameof(VariableDeclaration))]
[JsonDerivedType(typeof(ExternalStatement), nameof(ExternalStatement))]
[JsonDerivedType(typeof(WhileStatement), nameof(WhileStatement))]
[JsonDerivedType(typeof(SwitchStatement), nameof(SwitchStatement))]
[JsonDerivedType(typeof(IfStatement), nameof(IfStatement))]
[JsonDerivedType(typeof(AutoStatement), nameof(AutoStatement))]
[JsonDerivedType(typeof(FunctionCall), nameof(FunctionCall))]
public abstract record Statement() : AstNode;

public record VariableDeclaration(Symbol Symbol, Expression? Value) : Statement;

public record ExternalStatement(Symbol[] Externals) : Statement;

public record WhileStatement(BinaryExpression Condition, Statement[] Body) : Statement;

public record SwitchStatement(BinaryExpression Condition, Statement[] Body) : Statement;

public record IfStatement(BinaryExpression Condition, Statement[] Body, Statement[]? ElseBody) : Statement;

public record AutoStatement(Symbol[] Variables) : Statement;

public record FunctionCall(Symbol Symbol, Expression[] Parameters) : Statement;
