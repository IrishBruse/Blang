namespace IBlang;

public class Program
{
    public static void Main()
    {
        Compiler.Run("../Tests/Error.ib");
        Compiler.Run("../Tests/Syntax.ib");

        Compiler.Run("../Tests/HelloWorld.ib");
        Compiler.Run("../Tests/Fibonacci.ib");
    }
}
