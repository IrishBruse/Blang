#pragma warning disable IDE0005,CS8019
global using static GlobalUtilities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
#pragma warning restore IDE0005,CS8019

#pragma warning disable CA1050,CA2211,CS8714
public static class GlobalUtilities
{
    public static Dictionary<string, string> Flags = null!;

    public static void Log<K, V>(Dictionary<K, V> dictionary, [CallerArgumentExpression(nameof(dictionary))] string expression = "")
    {
        Console.WriteLine("Dictionary " + expression + ":");
        foreach (KeyValuePair<K, V> kv in dictionary)
        {
            Console.WriteLine("  " + kv.Key + ": " + kv.Value);
        }
        Console.WriteLine();
    }
}
#pragma warning restore CA1050,CA2211,CS8714
