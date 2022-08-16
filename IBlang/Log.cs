namespace IBlang;

using System;
using System.Runtime.CompilerServices;

public class Log
{
    public static void Throw(string message, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string method = "")
    {
        WriteLine($"Throw: {message} {file}:{lineNumber} -> {method}", ConsoleColor.Red);
        throw new CompilerDebugException($"Throw: {message} {file}:{lineNumber} -> {method}");
    }

    public static void Error(string message, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string method = "")
    {
        WriteLine($"Error: {message} {file}:{lineNumber} -> {method}", ConsoleColor.Red);
    }

    public static void Warn(string message)
    {
        WriteLine($"Warning: {message}", ConsoleColor.Yellow);
    }

    public static void Info(string message)
    {
        WriteLine($"Info: {message}", ConsoleColor.Blue);
    }

    public static void Assert(bool condition, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0, [CallerArgumentExpression("condition")] string expression = "", [CallerMemberName] string method = "")
    {
        if (!condition)
        {
            Error($"Assertion failed on {expression} {file}:{lineNumber} in {method}()");
        }
    }

    public static void WriteLine(string message, ConsoleColor color = ConsoleColor.Gray)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void Write(string message, ConsoleColor color = ConsoleColor.Gray)
    {
        Console.ForegroundColor = color;
        Console.Write(message);
        Console.ResetColor();
    }
}
