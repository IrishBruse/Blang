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
        Log($"FileAst: ({node.Functions.Length})");
    }

    public void Visit(FunctionDecleration node)
    {
        Console.WriteLine();
        Log("FunctionDecleration: " + node.Name);
    }

    public void Visit(BooleanExpression node)
    {
        Log($"BooleanExpression: {node.BooleanOperator}");
        Log($"  Left: {node.Left}");
        Log($"  Right: {node.Right}");
    }

    public void Visit(BinaryExpression node)
    {
        Log($"BinaryExpression: {node.BinaryOperator}");
    }

    public void Visit(IfStatement node)
    {
        Log("IfStatement: ");
    }

    public void Visit(ParameterDefinition node)
    {
        node.Deconstruct(out string type, out string name);
        Log($"ParameterDefinition: {type} {name}");
    }

    public void Visit(FunctionCallExpression node)
    {
        Log("FunctionCallExpression: " + node.Name + "()");
    }

    public void Visit(BooleanLiteral node)
    {
        Log("BooleanLiteral: " + node.Value);
    }

    public void Visit(StringLiteral node)
    {
        Log($"StringLiteral: \"{node.Value}\"");
    }

    public void Visit(IntegerLiteral node)
    {
        Log("IntegerLiteral: " + node.Value);
    }

    public void Visit(FloatLiteral node)
    {
        Log("FloatLiteral: " + node.Value);
    }

    public void Visit(Identifier node)
    {
        Log($"Identifier: {node.Name}");
    }

    public void Visit(ReturnStatement node)
    {
        Log($"Return:");
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
        Log($"AssignmentStatement: {node.Name} =");
    }

    public void Log(string msg)
    {
        PrintIndent();
        Console.WriteLine(msg);
    }
}
