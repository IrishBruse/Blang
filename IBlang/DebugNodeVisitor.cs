namespace IBlang;

public class DebugNodeVisitor : INodeVisitor
{
    private int indentation;

    public void Visit(FileAst file)
    {
        Console.WriteLine("-------- Debug Ast --------");
        foreach (FunctionDecleration function in file.Functions)
        {
            function.Accept(this);
        }
        Console.WriteLine("-------- Debug Ast --------");
    }

    public void Visit(FunctionDecleration functionDecleration)
    {
        Console.Write($"func {functionDecleration.Name}(");

        foreach (ParameterDefinition parameter in functionDecleration.Parameters)
        {
            parameter.Accept(this);
        }

        Console.WriteLine($")");

        Console.WriteLine("{");

        foreach (INode statement in functionDecleration.Statements)
        {
            Console.Write("    ");
            statement.Accept(this);
        }

        Console.WriteLine("}");
    }

    public void Visit(ParameterDefinition parameter)
    {
        Console.WriteLine("Parameter");
        Console.Write($"{parameter.Type} {parameter.Identifier}");
    }

    public void Visit(IfStatement ifStatement)
    {
        Console.Write("IfStatement");
    }

    public void Visit(BinaryExpression binaryExpression)
    {
        throw new NotImplementedException();
    }

    public void Visit(StringLiteral stringLiteral)
    {
        Console.Write(stringLiteral.Value);
    }

    public void Visit(IntegerLiteral integerLiteral)
    {
        throw new NotImplementedException();
    }

    public void Visit(FunctionCall functionCall)
    {
        Console.Write($"{functionCall.Name}(");
        foreach (INode item in functionCall.Args)
        {
            item.Accept(this);
        }
        Console.WriteLine(")");
    }

}
