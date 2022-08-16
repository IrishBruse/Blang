namespace IBlang.Stage5CodeGen;

using System.Text;

using IBlang.Stage2Parser;

public class CEmitter
{
    private StringBuilder writer;

    public CEmitter()
    {
        writer = new();
    }

    public StringBuilder Emit(Ast node)
    {
        writer = writer.Clear();
        Output(node);
        return writer;
    }

    private void Output(INode node)
    {
        switch (node)
        {
            case Ast expr:
            foreach (string item in File.ReadAllLines("./IBlang.c"))
            {
                WriteLine(item);
            }
            WriteLine();
            Visit(expr.FunctionDeclerations);
            break;

            case FunctionDecleration expr:
            Write("int "); // TODO replace with typechecker

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
            WriteLine("{\n");
            foreach (INode item in expr.Body)
            {
                Visit(item);
                Write(";");
            }
            WriteLine("\n}");
            break;

            case IfStatement expr:
            Write("if");
            Write("(");
            Visit(expr.Condition);
            Write(")");
            Visit(expr.Body);
            BlockStatement? elseBlock = expr.Else;
            if (elseBlock.HasValue)
            {
                Write("else");
                Visit(elseBlock.Value);
            }
            break;

            default:
            Log.Error($"Unhandled node {node}");
            throw new NotImplementedException(node.ToString());
        }
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
