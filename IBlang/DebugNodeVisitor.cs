namespace IBlang;

public class DebugNodeVisitor : INodeVisitor
{
    private int indentation;

    public void Visit(FileAst file)
    {
        foreach (FunctionDecleration function in file.Functions)
        {
            function.Accept(this);
        }
    }

    public void Visit(FunctionDecleration functionDecleration)
    {
        Console.Write($"{functionDecleration.Name}(");

        foreach (Parameter parameter in functionDecleration.Parameters)
        {
            parameter.Accept(this);
        }

        Console.WriteLine($")");

        Console.WriteLine("{");

        foreach (Statement statement in functionDecleration.Statements)
        {
            Console.WriteLine("    ");
            statement.Accept(this);
        }

        Console.WriteLine("}");
    }

    public void Visit(Parameter parameter)
    {
        Console.Write($"{parameter.Type} {parameter.Identifier}");
    }

    public void Visit(Statement statement)
    {
        Console.WriteLine("Statement");
    }

    public void Visit(IfStatement ifStatement)
    {

    }

    public void Visit(BinaryExpression binaryExpression)
    {

    }

    public void Visit(StringLiteral stringLiteral)
    {

    }

    public void Visit(FunctionCall functionCall)
    {
        Console.WriteLine($"{functionCall.Name}({string.Join(", ", functionCall.Args.Select(x => x.ToString()))})");
    }
}
