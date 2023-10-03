namespace IBlang;

public class Project
{
    List<CheckedFunction> functions = new();

    public void AddFunction(CheckedFunction function)
    {
        functions.Add(function);
    }

    public CheckedFunction GetFunction(int functionId)
    {
        return functions[functionId];
    }
}
