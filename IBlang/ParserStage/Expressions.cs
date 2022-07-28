namespace IBlang.ParserStage;

using IBlang.LexerStage;

public abstract record Node { }

public record FunctionCallExpression(string Identifier, Node[] Args) : Node;
public record GarbageExpression(Token ErrorToken) : Node;
public record BinaryExpression(Node Left, string Operator, Node Right) : Node;
public record AssignmentExpression(Identifier Left, Node Right) : Node;
public record ValueLiteral(ValueType Type, string Value) : Node;
public record Identifier(string Name) : Node;
public record ParameterDecleration(string Type, string Name) : Node;
public record FunctionDecleration(string Identifier, ParameterDecleration[] Parameters, BlockStatement Body) : Node;
public record IfStatement(Node Condition, BlockStatement Body, BlockStatement? Else) : Node;
public record BlockStatement(Node[] Body) : Node;
public record ReturnStatement(Node Statement) : Node;
public record Ast(Node[] FunctionDeclerations) : Node;
