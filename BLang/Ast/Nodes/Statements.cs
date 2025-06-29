namespace BLang.Ast.Nodes;
using BLang.Utility;

public abstract record Statement() : AstNode;

public record GlobalVariable(Symbol Symbol, int? Value) : Statement;

public record ExternalStatement(Symbol[] Externals) : Statement;

public record WhileStatement(BinaryExpression Condition, Statement[] Body) : Statement;

public record IfStatement(BinaryExpression Condition, Statement[] Body, Statement[]? ElseBody) : Statement;

public record AutoStatement(Symbol[] Variables) : Statement;

public record FunctionCall(Symbol Symbol, Expression[] Parameters) : Statement;

public record VariableDeclarator(Symbol Symbol, Expression Value) : Statement;
