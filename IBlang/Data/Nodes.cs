namespace IBlang;

using IBlang.Data;

using OneOf;

public interface INode { }

public record FileAst(FunctionDecleration[] Functions, string Path) : INode { }

public record FunctionDecleration(string Name, Token ReturnType, ParameterDefinition[] Parameters, BlockBody Body) : INode { }
public record IfStatement(BooleanExpression Condition, BlockBody Body, BlockBody? ElseBody = null) : INode { }
public record ReturnStatement(Expression Result) : INode { }
public record ParameterDefinition(string Type, string Name) : INode { }

public record FunctionCallExpression(string Name, Expression[] Args) : INode { }
public record FunctionCallStatement(string Name, Expression[] Args) : FunctionCallExpression(Name, Args) { }
public record BinaryExpression(Token BinaryOperator, Expression Left, Expression Right) : INode { }
public record BooleanExpression(Token BooleanOperator, Expression Left, Expression Right) : INode { }
public record AssignmentStatement(string Name, Expression Value) : INode { }

public record BooleanLiteral(bool Value) : INode { }
public record StringLiteral(string Value) : INode { }
public record IntegerLiteral(int Value) : INode { }
public record FloatLiteral(float Value) : INode { }
public record Identifier(string Name) : INode { }
public record BlockBody(Statement[] Statements) : INode { }
public record Error(string Value) : INode { }

public record Type(string Value) { }


[GenerateOneOf]
public partial class Statement : OneOfBase<IfStatement, FunctionCallStatement, ReturnStatement, AssignmentStatement, Error>, INode { }

[GenerateOneOf]
public partial class Expression : OneOfBase<StringLiteral, FloatLiteral, IntegerLiteral, Identifier, BinaryExpression, BooleanExpression, FunctionCallExpression, Error>, INode { }

public interface IVisitor
{
    public int Indent { get; set; }

    public void Visit(FileAst node);
    public void Visit(FunctionDecleration node);
    public void Visit(BinaryExpression node);
    public void Visit(BooleanExpression node);
    public void Visit(ParameterDefinition node);
    public void Visit(FunctionCallExpression node);

    public void Visit(IfStatement node);
    public void Visit(ReturnStatement node);
    public void Visit(AssignmentStatement node);

    public void Visit(BooleanLiteral node);
    public void Visit(StringLiteral node);
    public void Visit(IntegerLiteral node);
    public void Visit(FloatLiteral node);
    public void Visit(Identifier node);
    public void Visit(Token node);

    public void Visit(Error node);
}
