namespace BLang;

using System;
using BLang.Utility;

public class Program
{
    public static void Main(string[] args)
    {
        if (Environment.GetEnvironmentVariable("DOTNET_WATCH") == "1")
        {
            Console.WriteLine(new string('\n', Console.WindowHeight));
        }

        Options.Parse(args);

        switch (options)
        {
            case TestOptions:
            Tester.Test();
            break;

            case RunOptions:
            Run();
            break;

            case FormatOptions:
            Run();
            break;

            case BuildOptions:
            Build();
            break;
        }
    }

    public static void Run()
    {
        var opt = (RunOptions)options;
        CompileOutput output = Compiler.Compile(opt.File);
        Error(output.Errors);

        Executable.Run(output.Executable);
    }


    public static void Build()
    {
        var opt = (BuildOptions)options;
        CompileOutput output = Compiler.Compile(opt.File);
        Error(output.Errors);
    }
}
