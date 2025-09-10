namespace BLang;

using BLang.Utility;

public class Program
{
    public static void Main(string[] args)
    {
        BaseOptions.Parse(args);

        switch (Options)
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
            default:
                break;
        }
    }

    public static void Run()
    {
        RunOptions opt = (RunOptions)Options;
        CompileOutput output = Compiler.Compile(opt.File);
        Error(output.Errors);

        _ = Executable.Run(output.Executable);
    }


    public static void Build()
    {
        BuildOptions opt = (BuildOptions)Options;
        CompileOutput output = Compiler.Compile(opt.File);
        Error(output.Errors);
    }
}
