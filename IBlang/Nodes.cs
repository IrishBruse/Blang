namespace IBlang;

public record FileAst(FunctionDecleration[] Functions) : INode
{
    public void Accept(INodeVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public record FunctionDecleration(string Name, ParameterDefinition[] Parameters, INode[] Statements) : INode
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

public record IfStatement(INode[] Parameters, INode[] Statements) : INode
{
    public void Accept(INodeVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public record ParameterDefinition(string Type, string Identifier) : INode
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

public record IntegerLiteral(int Value) : INode
{
    public void Accept(INodeVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public record FunctionCall(string Name, INode[] Args) : INode
{
    public void Accept(INodeVisitor visitor)
    {
        visitor.Visit(this);
    }
}

public record Garbage(string Error) : INode
{
    public void Accept(INodeVisitor visitor)
    {
        visitor.Visit(this);
    }
}
