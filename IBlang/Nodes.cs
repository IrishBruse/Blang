namespace IBlang;

using System.Collections;

using OneOf;
using OneOf.Types;

public interface INode { }

public record FileAst(FunctionDecleration[] Functions) : INode { }

public record FunctionDecleration(string Name, ParameterDefinition[] Parameters, BlockBody Body) : INode { }
public record IfStatement(BooleanExpression Condition, BlockBody Body, BlockBody? ElseBody = null) : INode { }
public record ReturnStatement(Expression Result) : INode { }
public record ParameterDefinition(string Type, string Identifier) : INode { }

public record FunctionCallExpression(string Name, Expression[] Args) : INode { }
public record BinaryExpression(Expression Left, Expression Right) : INode { }
public record BooleanExpression(Expression Left, Expression Right) : INode { }

public record BooleanLiteral(bool Value) : INode { }
public record StringLiteral(string Value) : INode { }
public record IntegerLiteral(int Value) : INode { }
public record FloatLiteral(float Value) : INode { }
public record Identifier(string Name) : INode { }
public record BlockBody(Statement[] Statements) : INode, IEnumerator<Statement>, IEnumerable<Statement>
{
    public BlockBody(List<Statement> statements) : this(statements.ToArray()) { }

    public Statement this[int key]
    {
        get => Statements[key];
        set => Statements[key] = value;
    }

    private int position = -1;

    object IEnumerator.Current => Current;
    public Statement Current => Statements[position];

    public bool MoveNext()
    {
        position++;
        return position < Statements.Length;
    }

    public void Reset()
    {
        position = -1;
    }

    public IEnumerator<Statement> GetEnumerator()
    {
        return this;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

[GenerateOneOf]
public partial class Statement : OneOfBase<IfStatement, FunctionCallExpression, ReturnStatement, Error<string>>, INode { }

[GenerateOneOf]
public partial class Expression : OneOfBase<StringLiteral, FloatLiteral, IntegerLiteral, Identifier, FunctionCallExpression, Error<string>>, INode { }

public interface IVisitor
{
    public int Indent { get; set; }

    public void Visit(FileAst node);
    public void Visit(FunctionDecleration node);
    public void Visit(BinaryExpression node);
    public void Visit(BooleanExpression node);
    public void Visit(IfStatement node);
    public void Visit(ParameterDefinition node);
    public void Visit(FunctionCallExpression node);

    public void Visit(BooleanLiteral node);
    public void Visit(StringLiteral node);
    public void Visit(IntegerLiteral node);
    public void Visit(FloatLiteral node);
    public void Visit(Identifier node);
    public void Visit(BlockBody node);

    public void Visit(Error<string> node);
}
