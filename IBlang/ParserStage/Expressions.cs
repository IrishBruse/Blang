namespace IBlang.ParserStage;

using IBlang.LexerStage;

public abstract record Node { }
public record FunctionCallExpression(string Identifier, ValueLiteral[] Args) : Node;
public record GarbageExpression(Token ErrorToken) : Node;
public record BinaryExpression(Node Left, Node Right) : Node;
public record ValueLiteral(ValueType Type, string Value) : Node;
public record Identifier(string Name) : Node;
public record FunctionDecleration(string Identifier, Node[] Parameters, Node[] Body) : Node;
public record Ast(Node[] FunctionDeclerations) : Node;

public enum ValueType
{
    String,
    Int,
    Float
}
