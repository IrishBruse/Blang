namespace IBlang;

using OneOf.Types;

public class PrintAstDebugger : IVisitor
{
    public int Indent { get; set; }

    void PrintIndent()
    {
        for (int i = 0; i < Indent; i++)
        {
            Console.Write("  ");
        }
    }

    public void Visit(FileAst node)
    {
        PrintIndent();
        Console.WriteLine($"FileAst: ({node.Functions.Length})");
    }

    public void Visit(FunctionDecleration node)
    {
        PrintIndent();
        Console.WriteLine("FunctionDecleration: " + node.Name);
    }

    public void Visit(BooleanExpression node)
    {
        PrintIndent();
        Console.WriteLine($"BooleanExpression: {node.BooleanOperator}");
    }

    public void Visit(BinaryExpression node)
    {
        PrintIndent();
        Console.WriteLine($"BinaryExpression: {node.BinaryOperator}");
    }

    public void Visit(IfStatement node)
    {
        PrintIndent();
        Console.WriteLine("IfStatement: ");
    }

    public void Visit(ParameterDefinition node)
    {
        PrintIndent();
        Console.WriteLine("ParameterDefinition: " + node);
    }

    public void Visit(FunctionCallExpression node)
    {
        PrintIndent();
        Console.WriteLine("FunctionCallExpression: " + node.Name + "()");
    }

    public void Visit(BooleanLiteral node)
    {
        PrintIndent();
        Console.WriteLine("BooleanLiteral: " + node.Value);
    }

    public void Visit(StringLiteral node)
    {
        PrintIndent();
        Console.WriteLine($"StringLiteral: \"{node.Value}\"");
    }

    public void Visit(IntegerLiteral node)
    {
        PrintIndent();
        Console.WriteLine("IntegerLiteral: " + node.Value);
    }

    public void Visit(FloatLiteral node)
    {
        PrintIndent();
        Console.WriteLine("FloatLiteral: " + node.Value);
    }

    public void Visit(Identifier node)
    {
        PrintIndent();
        Console.WriteLine($"Identifier: ({node.Name})");
    }

    public void Visit(ReturnStatement node)
    {
        PrintIndent();
        Console.WriteLine($"Return:");
    }

    public void Visit(Error<string> node)
    {
        PrintIndent();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Error: " + node.Value);
        Console.ResetColor();
    }

    public void Visit(AssignmentStatement node)
    {
        PrintIndent();
        Console.WriteLine($"AssignmentStatement: {node.Name} =");
    }
}
