namespace IBlang.Stage4CodeGen;

using System.Text;

using IBlang.Stage2Parser;

public class CEmitter
{
    private readonly Context ctx;
    private int depth;
    private StringBuilder writer;

    public CEmitter(Context ctx)
    {
        writer = new();
        depth = -1;
        this.ctx = ctx;
    }

    public StringBuilder Emit(INode node)
    {
        writer = writer.Clear();
        Output(node);
        return writer;
    }

    public void Output(INode node)
    {
        depth++;
        for (int i = 0; i < depth; i++)
        {
            Console.Write("    ");
        }

        Console.WriteLine(node);

        switch (node)
        {
            case Ast expr:
            WriteLine("#define IBLANG_IMPLEMENTATION");
            foreach (string item in File.ReadAllLines("./IBlang.c"))
            {
                WriteLine(item);
            }
            WriteLine();
            Visit(expr.FunctionDeclerations);
            break;

            case FunctionDecleration expr:
            // TODO replace with typechecker
            Write("int ");

            if (expr.Identifier == "Main")
            {
                Write("main");
            }
            else
            {
                Write($"{expr.Identifier}");
            }

            Write("(");
            foreach (ParameterDecleration param in expr.Parameters)
            {
                Write(param.Type);
                Write(" ");
                Write(param.Name);
            }
            Write(")");
            Visit(expr.Body);
            break;

            case FunctionCallExpression expr:
            Write($"{expr.Identifier}(");
            Visit(expr.Args);
            Write(")");
            break;

            case BinaryExpression expr:
            Output(expr.Left);
            Write(expr.Operator);
            Output(expr.Right);
            break;

            case AssignmentExpression expr:
            Write("int ");// TODO replace with typechecker
            Write(expr.Left.Name);
            Write("=");
            Output(expr.Right);
            break;

            case ReturnStatement expr:
            Write("return ");
            Output(expr.Statement);
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
            Write($"{expr.Name}");
            break;

            case BlockStatement expr:
            WriteLine("{\n");
            foreach (INode item in expr.Body)
            {
                Output(item);
                Write(";");
            }
            WriteLine("\n}");
            break;

            case IfStatement expr:
            Write("if");
            Write("(");
            Output(expr.Condition);
            Write(")");
            Visit(expr.Body);
            if (expr.Else != null)
            {
                Write("else");
                Output(expr.Else);
            }
            break;

            default:
            Log.Error($"Unhandled node {node}");
            throw new NotImplementedException(node.ToString());
        }

        depth--;
    }

    private void Visit<T>(params T[] nodes) where T : INode
    {
        foreach (T item in nodes)
        {
            Output(item);
        }
    }

    private void Write(string value) => _ = writer.Append(value);

    private void WriteLine(string value) => _ = writer.AppendLine(value);

    private void WriteLine() => _ = writer.AppendLine("");
}
