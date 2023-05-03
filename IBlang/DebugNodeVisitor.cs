namespace IBlang;

public class DebugNodeVisitor : INodeVisitor
{
    public void Visit(Ast ast)
    {
        foreach (FunctionDecleration function in ast.Functions)
        {
            function.Accept(this);
        }
    }

    public void Visit(FunctionDecleration functionDecleration)
    {
        foreach (Parameter parameter in functionDecleration.Parameters)
        {
            parameter.Accept(this);
        }

        foreach (Statement statement in functionDecleration.Statements)
        {
            statement.Accept(this);
        }
    }

    public void Visit(Parameter parameter)
    {

    }

    public void Visit(Statement statement)
    {

    }
}
