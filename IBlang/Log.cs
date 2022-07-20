namespace IBlang;

using System.Runtime.CompilerServices;

public class Log
{
    public static void Error(string error, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{file}:{lineNumber} {error}");
        Console.ResetColor();
    }

    public static void Assert(bool condition, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0, [CallerArgumentExpression("condition")] string expression = "")
    {
        if (!condition)
        {
            Error($"Assertion failed on {expression}", file, lineNumber);
        }
    }
}
