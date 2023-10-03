namespace IBlang.Walker;

using System.Diagnostics;
using System.Text;

public class Transpiler
{
    StringBuilder output = new();
    int indention;
    readonly Project project;

    public Transpiler(Project project)
    {
        this.project = project;
    }

    void Indent()
    {
        indention++;
    }

    void Dedent()
    {
        indention--;
    }

    public void TranspileToC(FileAst file)
    {
        Prelude();

        foreach (FunctionDecleration function in file.Functions)
        {
            EmitCFunctionHeader(function);
            WriteLine(";");
        }

        foreach (FunctionDecleration function in file.Functions)
        {
            Emit(function);
        }

        File.WriteAllText(Path.ChangeExtension(file.Path, "c"), output.ToString());
    }


    public void Compile(FileAst file)
    {
        Prelude();

        foreach (FunctionDecleration function in file.Functions)
        {
            WriteLineIndented();
            Emit(function);
        }

        Process? tcc = Process.Start(new ProcessStartInfo()
        {
            FileName = "tcc",
            Arguments = "-Wall " + Path.ChangeExtension(file.Path, "c"),
            RedirectStandardInput = true,
        });

        if (tcc == null)
        {
            Console.WriteLine("Failed to launch tcc");
            return;
        }

        tcc.StandardInput.Write(output);
    }

    void Prelude()
    {
        foreach (string text in File.ReadAllLines("Prelude/Prelude.c"))
        {
            WriteLine(text);
        }
        WriteLine();
    }

    void Emit(FunctionDecleration function)
    {
        WriteLineIndented();
        EmitCFunctionHeader(function);
        WriteLine();
        Emit(function.Body);
    }

    void EmitCFunctionHeader(FunctionDecleration function)
    {
        string name = function.Name;

        if (name == "Main")
        {
            name = "main";
        }

        string parameters = string.Join(", ", function.Parameters.Select(p => p.Type + " " + p.Name));

        Write($"{function.ReturnType.Value} {name}({parameters})");
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
        Write($"\"{expression.Value}\"");
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

    void Emit(FunctionCallExpression expression)
    {
        Write(expression.Name + "(");
        bool start = true;
        foreach (Expression arg in expression.Args)
        {
            if (!start)
            {
                Write(", ");
            }

            EmitExpression(arg);
            start = false;
        }
        Write(")");
    }

    void Emit(FunctionCallStatement statement)
    {
        WriteIndention();
        Emit((FunctionCallExpression)statement);
        Semicolon();
    }

    void Emit(ReturnStatement statement)
    {
        WriteIntend("return ");
        EmitExpression(statement.Result);
        Semicolon();
    }

    void Emit(AssignmentStatement statement)
    {
        WriteIntend($"int {statement.Name} = ");
        EmitExpression(statement.Value);
        Semicolon();
    }

    void Semicolon()
    {
        WriteLine(";");
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
        WriteIndention();
        _ = output.Append(text);
    }

    void Write(string? text = null)
    {
        _ = output.Append(text);
    }

    void WriteLineIndented(string? text = null)
    {
        WriteIndention();
        _ = output.Append(text + "\n");
    }

    void WriteIndention()
    {
        for (int i = 0; i < indention; i++)
        {
            _ = output.Append("    ");
        }
    }

    void WriteLine(string? text = null)
    {
        _ = output.Append(text + "\n");
    }
}
