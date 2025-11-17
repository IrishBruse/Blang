namespace BLang;

using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BLang.Utility;

public class Program
{
    private static Option<bool> VerboseFlag = new("--verbose", "-v")
    {
        Description = "Verbose output",
    };

    private static Option<bool> VeryVerboseFlag = new("-vv")
    {
        Description = "Very Verbose output",
    };

    private static Option<bool> VeryVeryVerboseFlag = new("-vvv")
    {
        Description = "Very Very Verbose output",
    };

    private static Option<bool> AstFlag = new("--ast")
    {
        Description = "Dump compiler ast information",
    };

    private static Option<bool> TokensFlag = new("--tokens")
    {
        Description = "Print tokens as they are consumed",
    };

    private static Option<bool> SymbolsFlag = new("--symbols")
    {
        Description = "Print symbol table",
    };

    private static Option<bool> MemoryFlag = new("--memory")
    {
        Description = "Output memory registers in output comments",
    };

    private static Argument<string> BuildFileArg = new("file")
    {
        Description = "Path to b file to build",
    };

    private static Argument<string> RunFileArg = new("file")
    {
        Description = "Path to b file to run",
    };

    // Test Flags

    private static Argument<string> TestFileArg = new("file")
    {
        Description = "Path to b test file",
        DefaultValueFactory = (_) => ""
    };

    private static Option<bool> UpdateSnapshotFlag = new("--updateSnapshot", "-u")
    {
        Description = "Update tests snapshot",
    };

    public static int Main(string[] args)
    {
        RootCommand rootCommand = BuildCommand();
        rootCommand.Subcommands.Add(RunCommand());
        rootCommand.Subcommands.Add(TestCommand());

        ParseResult result = rootCommand.Parse(args);

        return result.Invoke();
    }

    private static RootCommand BuildCommand()
    {
        RootCommand rootCommand = new("Compiler for the b programming lanaugage");

        rootCommand.Add(BuildFileArg);

        GlobalFlags(rootCommand);

        rootCommand.SetAction(Build);
        return rootCommand;
    }

    private static int Build(ParseResult parseResult)
    {
        Options.Verb = Verb.Build;
        ParseFlags(parseResult);

        string file = parseResult.GetValue(BuildFileArg)!;

        CompileOutput output = Compiler.Compile(file);

        if (!output.Success)
        {
            return -1;
        }

        if (Options.Ast)
        {
            output.WriteAst();
        }

        return 0;
    }

    private static Command RunCommand()
    {
        Command runCommand = new("run", "Run .b file");

        GlobalFlags(runCommand);

        // Args
        runCommand.Add(RunFileArg);

        runCommand.SetAction(Run);
        return runCommand;
    }


    private static int Run(ParseResult parseResult)
    {
        Options.Verb = Verb.Run;
        ParseFlags(parseResult);

        string file = parseResult.GetValue(RunFileArg)!;

        CompileOutput output = Compiler.Compile(file);

        if (output.Success)
        {
            Process process = Process.Start(output.Executable!);
            process.WaitForExit();
            return process.ExitCode;
        }
        else
        {
            Error(output.Error);
            return 1;
        }
    }


    private static Command TestCommand()
    {
        Command testCommand = new("test", "Test compiler output");

        GlobalFlags(testCommand);

        // Args
        testCommand.Add(TestFileArg);

        testCommand.Add(UpdateSnapshotFlag);
        testCommand.SetAction(Test);
        return testCommand;
    }

    private static int Test(ParseResult parseResult)
    {
        Options.Verb = Verb.Test;
        ParseFlags(parseResult);

        string? file = parseResult.GetValue(TestFileArg);

        if (file != null && file != "")
        {
            _ = Tester.TestFile(file);
        }
        else
        {
            string[] exampleFiles = Directory.GetFiles("Examples/", "*.b", SearchOption.AllDirectories);
            string[] testFiles = Directory.GetFiles("Tests/", "*.b", SearchOption.AllDirectories);
            string[] tests = exampleFiles.Concat(testFiles).ToArray();

            Tester.TestFiles(tests);
        }

        return 0;
    }

    private static void GlobalFlags(Command rootCommand)
    {
        rootCommand.Add(TokensFlag);
        rootCommand.Add(SymbolsFlag);
        rootCommand.Add(MemoryFlag);
        rootCommand.Add(AstFlag);

        rootCommand.Add(VerboseFlag);
        rootCommand.Add(VeryVerboseFlag);
        rootCommand.Add(VeryVeryVerboseFlag);
    }

    private static void ParseFlags(ParseResult parseResult)
    {
        int verboseLevel = 0;

        verboseLevel = parseResult.GetValue(VerboseFlag) ? 1 : verboseLevel;
        verboseLevel = parseResult.GetValue(VeryVerboseFlag) ? 2 : verboseLevel;
        verboseLevel = parseResult.GetValue(VeryVeryVerboseFlag) ? 3 : verboseLevel;

        Options.Verbose = verboseLevel;
        Options.Ast = parseResult.GetValue(AstFlag);
        Options.Tokens = parseResult.GetValue(TokensFlag);
        Options.Symbols = parseResult.GetValue(SymbolsFlag);
        Options.Memory = parseResult.GetValue(MemoryFlag);

        Options.UpdateSnapshots = parseResult.GetValue(UpdateSnapshotFlag);
    }
}
