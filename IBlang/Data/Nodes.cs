namespace IBlang;

using IBlang.Data;

using OneOf;

public record FileAst(FunctionDecleration[] Functions, string Path);

public record FunctionDecleration(string Name, Token ReturnType, ParameterDefinition[] Parameters, BlockBody Body);
public record IfStatement(BooleanExpression Condition, BlockBody Body, BlockBody? ElseBody = null);
public record ReturnStatement(Expression Result);
public record ParameterDefinition(string Type, string Name);

public record FunctionCallExpression(string Name, Expression[] Args);
public record FunctionCallStatement(string Name, Expression[] Args) : FunctionCallExpression(Name, Args);
public record BinaryExpression(Token BinaryOperator, Expression Left, Expression Right);
public record BooleanExpression(Token BooleanOperator, Expression Left, Expression Right);
public record AssignmentStatement(string Name, Expression Value);

public record BooleanLiteral(bool Value);
public record StringLiteral(string Value);
public record IntegerLiteral(int Value);
public record FloatLiteral(float Value);
public record Identifier(string Name);
public record BlockBody(Statement[] Statements);
public record Error(string Message);

public record Type(string Value);

[GenerateOneOf]
public partial class Statement : OneOfBase<IfStatement, FunctionCallStatement, ReturnStatement, AssignmentStatement, Error>;

[GenerateOneOf]
public partial class Expression : OneOfBase<StringLiteral, FloatLiteral, IntegerLiteral, Identifier, BinaryExpression, BooleanExpression, FunctionCallExpression, Error>;

public interface IVisitor
{
    public int Depth { get; set; }

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
