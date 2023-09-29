namespace IBlang;

public class TypeChecker
{

    public void Check(FileAst ast)
    {
        Console.WriteLine("-------- TypeChecker --------");

    }

    void Check(FunctionDecleration function)
    {
        Console.WriteLine($"Checking function {function.Name}");

    }
}
