#pragma warning disable CS8019
global using static BLang.Utility.Globals;
#pragma warning restore CS8019

namespace BLang.Utility;

using System;

public class Globals
{
    private static string prefix = "";

    static void Print(string? message)
    {
        message = message?.Trim();
        if (!string.IsNullOrEmpty(message))
        {
            foreach (string line in message.Split("\n"))
            {
                Console.WriteLine(prefix + line);
            }
        }
    }

    public static void Log(string? message, string? prefix = "")
    {
        SetPrefix(prefix);
        Print(message);
        SetPrefix();
    }

    public static void Error(string? message, string? prefix = "")
    {
        SetPrefix(prefix);
        Console.ForegroundColor = ConsoleColor.Red;
        Print(message);
        Console.ResetColor();
        SetPrefix();
    }

    public static void Debug(string? message, string prefix = "")
    {
        SetPrefix(prefix);
        Console.ForegroundColor = ConsoleColor.Blue;
        Print(message);
        Console.ResetColor();
        SetPrefix();
    }

    public static void SetPrefix(string? prefix = "")
    {
        if (prefix != null)
        {
            Globals.prefix = $"[{prefix}] ";
        }
        else
        {
            Globals.prefix = "";
        }
    }
}
