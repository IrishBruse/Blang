#pragma warning disable CS8019
global using static BLang.Utility.Globals;
#pragma warning restore CS8019

namespace BLang.Utility;

using System;

public class Globals
{
    private static void Print(string? message, string? prefix, ConsoleColor? color = null)
    {
        if (message != null)
        {
            if (color != null) Console.ForegroundColor = (ConsoleColor)color;
            foreach (string line in message.Split("\n"))
            {
                string pre = string.IsNullOrEmpty(prefix) ? "" : $"[{prefix}] ";
                Console.Error.WriteLine(pre + line);
            }
            if (color != null) Console.ResetColor();
        }
    }

    public static void Log(string? message, string? prefix = null, ConsoleColor? color = null)
    {
        Print(message, prefix, color);
    }

    public static void Error(string? message, string? prefix = null)
    {
        if (!string.IsNullOrEmpty(message))
        {
            Console.Error.WriteLine(Colors.Red(message));
        }
    }

    public static void Debug(string? message, string? prefix = null, ConsoleColor? color = ConsoleColor.DarkGray)
    {
        if (Options.Verbose > 1)
        {
            Print(message, prefix, color);
        }
    }

    public static void Info(string? message, string? prefix = null)
    {
        Print(message, prefix, ConsoleColor.Blue);
    }
}
