#pragma warning disable CS8019
global using static BLang.Utility.Globals;
#pragma warning restore CS8019

namespace BLang.Utility;

using System;

public class Globals
{
    public static Options options = null!;
    private static string logPrefix = "";

    static void Print(string? message, string? prefix, ConsoleColor? color = null)
    {
        SetPrefix(prefix);
        message = message?.Trim();
        if (!string.IsNullOrEmpty(message))
        {
            if (color != null) Console.ForegroundColor = (ConsoleColor)color;
            foreach (string line in message.Split("\n"))
            {
                Console.WriteLine(logPrefix + line);
            }
            if (color != null) Console.ResetColor();
        }
        SetPrefix();
    }

    public static void Log(string? message, string? prefix = null)
    {
        Print(message, prefix);
    }

    public static void Error(string? message, string? prefix = null)
    {
        Print(message, prefix, ConsoleColor.Red);
    }

    public static void Debug(string? message, string? prefix = null)
    {
        Print(message, prefix, ConsoleColor.DarkGray);
    }

    public static void Info(string? message, string? prefix = null)
    {
        Print(message, prefix, ConsoleColor.Blue);
    }

    public static void SetPrefix(string? prefix = null)
    {
        if (prefix != null)
        {
            logPrefix = $"[{prefix}] ";
        }
        else
        {
            logPrefix = "";
        }
    }
}
