namespace IBlang;

public record Ast(FunctionDecleration[] Functions) : INode
{
    public void Accept(INodeVisitor visitor)
    {
        visitor.Visit(this);
    }
}
public record FunctionDecleration(Parameter[] Parameters, Statement[] Statements) : INode
{
    public void Accept(INodeVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public record Parameter() : INode
{
    public void Accept(INodeVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public record Statement() : INode
{
    public void Accept(INodeVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public interface INode
{
    public void Accept(INodeVisitor visitor);
}

public interface INodeVisitor
{
    public void Visit(Ast ast);
    public void Visit(FunctionDecleration functionDecleration);
    public void Visit(Parameter parameter);
    public void Visit(Statement statement);
}
