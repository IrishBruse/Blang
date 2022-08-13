namespace IBlang.Stage2Parser;

using IBlang.Stage1Lexer;

public interface INode { }

public record struct FunctionCallExpression(string Identifier, INode[] Args) : INode;
public record struct GarbageExpression(Token ErrorToken) : INode;
public record struct BinaryExpression(INode Left, string Operator, INode Right) : INode;
public record struct AssignmentExpression(Identifier Left, INode Right) : INode;
public record struct ValueLiteral(ValueType Type, string Value) : INode;
public record struct Identifier(string Name) : INode;
public record struct ParameterDecleration(string Type, string Name) : INode;
public record struct FunctionDecleration(string Identifier, ParameterDecleration[] Parameters, BlockStatement Body) : INode;
public record struct IfStatement(INode Condition, BlockStatement Body, BlockStatement? Else) : INode;
public record struct BlockStatement(INode[] Body) : INode;
public record struct ReturnStatement(INode Statement) : INode;
public record struct Ast(FunctionDecleration[] FunctionDeclerations) : INode;
