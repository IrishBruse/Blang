namespace IBlang.Walker;

public class PrintAstDebugger : IVisitor
{
    public int Depth { get; set; }

    void PrintIndent()
    {
        for (int i = 0; i < Depth; i++)
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
        Log($"FunctionDecleration: {node.ReturnType.Value} {node.Name}()");
    }

    public void Visit(BooleanExpression node)
    {
        Data.Token op = node.BooleanOperator;
        Log($"BooleanExpression: {op.Type} {op.Value}");
        Log($"  Left: {node.Left}");
        Log($"  Right: {node.Right}");
    }

    public void Visit(BinaryExpression node)
    {
        Data.Token op = node.BinaryOperator;
        Log($"BinaryExpression: {op.Type} {op.Value}");
        Log($"  Left: {node.Left}");
        Log($"  Right: {node.Right}");
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
    public void Visit(Data.Token node)
    {
        Log($"Token: {node.Type} {node.Value}");
    }

    public void Visit(ReturnStatement node)
    {
        Log($"Return:");
    }

    public void Visit(Error node)
    {
        PrintIndent();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Error: " + node.Message);
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
