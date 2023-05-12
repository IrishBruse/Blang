namespace IBlang;

public record FileAst(FunctionDecleration[] Functions) : INode
{
    public void Accept(INodeVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public record FunctionDecleration(string Name, Parameter[] Parameters, Statement[] Statements) : INode
{
    public void Accept(INodeVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public record BinaryExpression(INode Left, INode Right) : INode
{
    public void Accept(INodeVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public record IfStatement(Parameter[] Parameters, Statement[] Statements) : Statement
{
    public new void Accept(INodeVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public record Parameter(string Type, string Identifier) : INode
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

public record StringLiteral(string Value) : INode
{
    public void Accept(INodeVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public record FunctionCall(string Name, INode[] Args) : Statement
{
    public new void Accept(INodeVisitor visitor)
    {
        visitor.Visit(this);
    }
}
