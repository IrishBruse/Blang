namespace IBlang;

public interface INodeVisitor
{
    public void Visit(FileAst file);
    public void Visit(FunctionDecleration functionDecleration);
    public void Visit(Parameter parameter);
    public void Visit(Statement statement);
}
