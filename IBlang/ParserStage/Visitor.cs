namespace IBlang.ParserStage;

using System.IO;

public class Visitor
{
    private StreamWriter writer;

    int depth = -1;

    public Visitor(StreamWriter writer)
    {
        this.writer = writer;
    }

    public void Visit(Node node)
    {
        depth++;
        for (int i = 0; i < depth; i++)
        {
            Console.Write("  ");
        }
        Console.WriteLine(node.GetType().Name);
        switch (node)
        {
            case Ast expr:
            WriteLine("#define IBLANG_IMPLEMENTATION");
            WriteLine("#include \"IBlang.h\"");
            Visit(expr.FunctionDeclerations);
            break;

            case FunctionDecleration expr:
            if (expr.Identifier == "Main")
            {
                WriteLine($"int main(");
            }
            else
            {
                WriteLine($"{expr.Identifier}(");
            }

            Visit(expr.Parameters);
            Write(")");

            WriteLine("{");
            {
                Visit(expr.Body);
            }
            WriteLine("}");
            break;

            case FunctionCallExpression expr:
            Write($"{expr.Identifier}(");
            Visit(expr.Args);
            WriteLine(");");
            break;

            case BinaryExpression expr:
            Visit(expr.Left);
            Write("=");
            Visit(expr.Right);
            Write(";");
            break;

            case ValueLiteral expr:
            switch (expr.Type)
            {
                case ValueType.String:
                Write($"\"{expr.Value}\"");
                break;

                case ValueType.Int:
                Write($"{expr.Value}");
                break;

                case ValueType.Float:
                Write($"{expr.Value}f");
                break;
            }
            break;

            case Identifier expr:
            Write($"int {expr.Name}");
            break;

            default:
            Console.WriteLine($"ERROR: Unhandled node {node}");
            break;
        }

        depth--;
    }

    void Visit(params Node[] nodes)
    {
        foreach (var item in nodes)
        {
            Visit(item);
        }
    }

    private void Write(string value)
    {
        writer.Write(value);
    }

    private void WriteLine(string value)
    {
        writer.WriteLine(value);
    }
}
