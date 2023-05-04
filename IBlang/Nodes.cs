namespace IBlang;

public record File(FunctionDecleration[] Functions) : INode
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
