namespace IBlang.Walker;

using System.Text;

public class Transpiler
{
    StringBuilder output = new();

    public Transpiler() { }

    int indention;

    void Indent()
    {
        indention++;
    }

    void Dedent()
    {
        indention--;
    }

    public void Emit(FileAst file)
    {
        Prelude();

        foreach (FunctionDecleration function in file.Functions)
        {
            WriteLineIndented();
            Emit(function);
        }

        File.WriteAllText(Path.ChangeExtension(file.Path, "c"), output.ToString());
    }

    void Prelude()
    {
        WriteLineIndented("#include <stdio.h>");
    }

    void Emit(FunctionDecleration function)
    {
        WriteLine($"void {function.Name}({string.Join(", ", function.Parameters.Select(p => p.Type + " " + p.Identifier))})");
        Emit(function.Body);
    }

    void Emit(BlockBody block)
    {
        WriteLineIndented("{");

        Indent();
        foreach (Statement statement in block.Statements)
        {
            EmitStatement(statement);
        }
        Dedent();

        WriteLineIndented("}");
    }

    void Emit(StringLiteral expression)
    {
        Write(expression.Value);
    }

    void Emit(FloatLiteral expression)
    {
        Write(expression.Value.ToString());
    }

    void Emit(IntegerLiteral expression)
    {
        Write(expression.Value.ToString());
    }

    void Emit(Identifier expression)
    {
        Write(expression.Name);
    }

    void Emit(BinaryExpression condition)
    {
        EmitExpression(condition.Left);
        Write($" {condition.BinaryOperator.Value} ");
        EmitExpression(condition.Right);
    }

    void Emit(BooleanExpression condition)
    {
        EmitExpression(condition.Left);
        Write($" {condition.BooleanOperator.Value} ");
        EmitExpression(condition.Right);
    }

    void Emit(IfStatement statement)
    {
        WriteIntend($"if (");
        EmitExpression(statement.Condition);
        WriteLine(")");

        Emit(statement.Body);
        if (statement.ElseBody != null)
        {
            WriteLineIndented("else");
            Emit(statement.ElseBody);
        }
    }

    void Emit(FunctionCallExpression statement)
    {
        WriteIntend(statement.Name + "(");
        bool start = true;
        foreach (Expression arg in statement.Args)
        {
            if (!start)
            {
                Write(", ");
            }

            EmitExpression(arg);
            start = false;
        }
        WriteLine(");");
    }

    void Emit(ReturnStatement statement)
    {
        WriteIntend("return ");
        EmitExpression(statement.Result);
        WriteLine(";");
    }

    void Emit(AssignmentStatement statement)
    {
        WriteLine("AssignmentStatement");
    }

    void Emit(Error statement)
    {
        Write("// Error: " + statement.Value + "\n");
    }

    void EmitStatement(Statement statement)
    {
        statement.Switch(
            Emit,
            Emit,
            Emit,
            Emit,
            Emit
        );
    }

    void EmitExpression(Expression expression)
    {
        expression.Switch(
            Emit,
            Emit,
            Emit,
            Emit,
            Emit,
            Emit,
            Emit,
            Emit
        );
    }

    void WriteIntend(string? text = null)
    {
        for (int i = 0; i < indention; i++)
        {
            _ = output.Append("    ");
        }

        _ = output.Append(text);
    }

    void Write(string? text = null)
    {
        _ = output.Append(text);
    }

    void WriteLineIndented(string? text = null)
    {
        for (int i = 0; i < indention; i++)
        {
            _ = output.Append("    ");
        }

        _ = output.Append(text + "\n");
    }

    void WriteLine(string? text = null)
    {
        _ = output.Append(text + "\n");
    }
}
