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
[JsonDerivedType(typeof(GlobalVariable), nameof(GlobalVariable))]
public abstract record Statement() : AstNode;

[JsonDerivedType(typeof(VariableDeclaration), nameof(VariableDeclaration))]
[JsonDerivedType(typeof(ArrayDeclaration), nameof(ArrayDeclaration))]
public record GlobalVariable : Statement;

public record VariableDeclaration(Symbol Symbol, Expression? Value) : GlobalVariable;
public record ArrayDeclaration(Symbol Symbol, int Size, int[] Values) : GlobalVariable;

public record ExternalStatement(Symbol[] Externals) : Statement;

public record WhileStatement(BinaryExpression Condition, Statement[] Body) : Statement;

public record SwitchStatement(BinaryExpression Condition, Statement[] Body) : Statement;

public record IfStatement(BinaryExpression Condition, Statement[] Body, Statement[]? ElseBody) : Statement;

public record AutoStatement(VariableAssignment[] Variables) : Statement;
public record VariableAssignment(Symbol Symbol, int Value);

public record FunctionCall(Symbol Symbol, Expression[] Parameters) : Statement;
