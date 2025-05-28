namespace IBlang.Utility;

using System;
using System.Collections.Generic;
using System.IO;

public static class Globals
{
    static readonly Dictionary<string, string> flags = [];

    public static void ParseArgs(string[] args, out string? file)
    {
        file = null;

        int index = 0;
        string? Peek() { return index < args.Length ? args[index] : null; }
        string Next() { return args[index++]; }

        while (index < args.Length)
        {
            string? peek = Peek();
            if (peek != null && peek.StartsWith("--"))
            {
                string flagName = Next()[2..];

                peek = Peek();
                if (peek != null && !peek.StartsWith("--"))
                {
                    flags.Add(flagName, Next());
                }
                else
                {
                    flags.Add(flagName, "true");
                }
            }
            else if (peek != null)
            {
                file = Path.GetFullPath(Next());
            }
            else
            {
                break;
            }
        }

        Flags.Target = flags.GetValueOrDefault("target", "");

        Flags.Help = flags.GetValueOrDefault("help") == "true";
        Flags.ListTargets = flags.GetValueOrDefault("list-targets") == "true";
        Flags.Run = flags.GetValueOrDefault("run") == "true";
        Flags.Debug = flags.GetValueOrDefault("debug") == "true";
        Flags.Test = flags.GetValueOrDefault("test") == "true";
        Flags.Ast = flags.GetValueOrDefault("ast") == "true";

        if (string.IsNullOrEmpty(Flags.Target))
        {
            Console.WriteLine("Target is required");
            Environment.Exit(-1);
        }
    }
}
