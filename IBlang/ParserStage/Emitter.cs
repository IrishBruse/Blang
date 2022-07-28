namespace IBlang.ParserStage;

using System.IO;

public class Emitter
{
    private readonly StreamWriter writer;
    private int depth = -1;

    public Emitter(StreamWriter writer)
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
        Console.WriteLine(node);
        switch (node)
        {
            case Ast expr:
            WriteLine("#define IBLANG_IMPLEMENTATION");
            WriteLine("#include \"IBlang.h\"");
            WriteLine("");
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
            Visit(expr.Left);
            Write(expr.Operator);
            Visit(expr.Right);
            break;

            case AssignmentExpression expr:
            Write("int ");// TODO replace with typechecker
            Write(expr.Left.Name);
            Write("=");
            Visit(expr.Right);
            break;

            case ReturnStatement expr:
            Write("return ");
            Visit(expr.Statement);
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
            WriteLine("{");
            foreach (Node item in expr.Body)
            {
                Visit(item);
                Write(";");
            }
            WriteLine("}");
            break;

            case IfStatement expr:
            Write("if");
            Write("(");
            Visit(expr.Condition);
            Write(")");
            Visit(expr.Body);
            if (expr.Else != null)
            {
                Write("else");
                Visit(expr.Else);
            }
            break;

            default:
            Log.Error($"Unhandled node {node}");
            throw new NotImplementedException(node.ToString());
        }

        depth--;
    }

    private void Visit(params Node[] nodes)
    {
        foreach (Node item in nodes)
        {
            Visit(item);
        }
    }

    private void Write(string value) => writer.Write(value);

    private void WriteLine(string value) => writer.WriteLine(value);
}
