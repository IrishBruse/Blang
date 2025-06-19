#pragma warning disable CS8019
global using static BLang.Utility.Globals;
#pragma warning restore CS8019

namespace BLang.Utility;

using System;

public class Globals
{
    public static Options options = null!;

    static void Print(string? message, string? prefix, ConsoleColor? color = null)
    {
        message = message?.Trim();
        if (!string.IsNullOrEmpty(message))
        {
            if (color != null) Console.ForegroundColor = (ConsoleColor)color;
            foreach (string line in message.Split("\n"))
            {
                string pre = string.IsNullOrEmpty(prefix) ? "" : $"[{prefix}]";
                Console.WriteLine(pre + line);
            }
            if (color != null) Console.ResetColor();
        }
    }

    public static void Log(string? message, string? prefix = null)
    {
        Print(message, prefix);
    }

    public static void Error(string? message, string? prefix = null)
    {
        Print(message, prefix, ConsoleColor.Red);
    }

    public static void Debug(string? message, string? prefix = null, ConsoleColor? color = ConsoleColor.DarkGray)
    {
        if (options.Debug)
        {
            Print(message, prefix, color);
        }
    }

    public static void Info(string? message, string? prefix = null)
    {
        Print(message, prefix, ConsoleColor.Blue);
    }
}
