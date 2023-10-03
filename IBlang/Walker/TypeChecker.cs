namespace IBlang.Walker;

public class TypeChecker
{
    readonly Project project;

    public TypeChecker(Project project)
    {
        this.project = project;
    }

    public void Check(FileAst ast)
    {
        foreach (FunctionDecleration func in ast.Functions)
        {
            Check(func);
        }
    }

    void Check(FunctionDecleration function)
    {
        Console.WriteLine($"Checking function {function.Name}");
        CheckedVariable[] parameters = function.Parameters.Select(p => new CheckedVariable(p.Name, new(p.Type))).ToArray();
        project.AddFunction(new CheckedFunction(function.Name, new(function.ReturnType.Value), parameters));
    }
}
