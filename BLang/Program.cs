namespace BLang;

using BLang.Utility;

public class Program
{
    public static void Main(string[] args)
    {
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
        RunOptions opt = (RunOptions)options;
        CompileOutput output = Compiler.Compile(opt.File);
        Error(output.Errors);

        Executable.Run(output.Executable);
    }


    public static void Build()
    {
        BuildOptions opt = (BuildOptions)options;
        CompileOutput output = Compiler.Compile(opt.File);
        Error(output.Errors);
    }
}
