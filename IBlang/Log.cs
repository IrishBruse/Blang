namespace IBlang;

using System.Runtime.CompilerServices;

public class Log
{
    public static void Error(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {message}");
        Console.ResetColor();
    }

    public static void Warn(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Warning: {message}");
        Console.ResetColor();
    }

    public static void Info(string message)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"Info: {message}");
        Console.ResetColor();
    }

    public static void Assert(bool condition, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0, [CallerArgumentExpression("condition")] string expression = "", [CallerMemberName] string method = "")
    {
        if (!condition)
        {
            Error($"Assertion failed on {expression} {file}:{lineNumber} in {method}()");
        }
    }
}
