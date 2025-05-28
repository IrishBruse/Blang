namespace IBlang;

using System;
using System.IO;
using IBlang.Utility;

public class Program
{
    public static void Main(string[] args)
    {
        Console.Clear();

        Globals.ParseArgs(args, out string? file);

        if (!Flags.Test)
        {
            Compiler.Compile(file);
        }
        else
        {
            Flags.Run = true;

            string[] files = Directory.GetFiles("../Examples/Tests/");

            foreach (string f in files)
            {
                Console.WriteLine($"----- {f} -----");
                Compiler.Compile(f);
                Console.WriteLine($"----- {f} -----");
                Console.WriteLine();
            }
        }
    }
}
