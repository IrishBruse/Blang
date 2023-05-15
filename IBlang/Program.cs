namespace IBlang;

public class Program
{
    public static void Main()
    {
        Compiler.Run("../Tests/Syntax.ib");
        // Compiler.Run("../Tests/HelloWorld.ib");
    }
}
