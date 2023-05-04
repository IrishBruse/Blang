namespace IBlang;

public interface INode
{
    public void Accept(INodeVisitor visitor);
}
