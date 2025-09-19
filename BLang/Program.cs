namespace BLang;

using System.CommandLine;
using System.Diagnostics;
using BLang.Utility;

public class Program
{
    private static Option<bool> DebugFlag = new("--debug")
    {
        Description = "Dump compiler debug information.",
    };

    private static Option<bool> TokensFlag = new("--tokens")
    {
        Description = "Print tokenizers tokens.",
    };

    private static Option<bool> SymbolsFlag = new("--symbols")
    {
        Description = "Print symbol table.",
    };

    private static Argument<string> FileArg = new("file")
    {
        Description = "Path to .b file.",
    };

    // Test Flags

    private static Argument<string> TestFileArg = new("file")
    {
        Description = "Path to .b test file.",
        DefaultValueFactory = (_) => ""
    };

    private static Option<bool> UpdateSnapshotFlag = new("--updateSnapshot", "-u")
    {
        Description = "Update tests snapshot.",
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
        RootCommand rootCommand = new("Compiler for the b programming lanaugage.");

        rootCommand.Add(FileArg);

        // Global Flags
        rootCommand.Add(DebugFlag);
        rootCommand.Add(TokensFlag);
        rootCommand.Add(SymbolsFlag);

        rootCommand.SetAction(Build);
        return rootCommand;
    }

    private static int Build(ParseResult parseResult)
    {
        Options.Verb = Verb.Build;
        ParseFlags(parseResult);

        string file = parseResult.GetValue(FileArg)!;

        CompileOutput output = Compiler.Compile(file);
        Error(output.Errors);

        return 0;
    }

    private static Command RunCommand()
    {
        Command runCommand = new("run", "Run .b file");

        // Global Flags
        runCommand.Add(DebugFlag);
        runCommand.Add(TokensFlag);
        runCommand.Add(SymbolsFlag);

        // Args
        runCommand.Add(FileArg);

        runCommand.SetAction(Run);
        return runCommand;
    }


    private static int Run(ParseResult parseResult)
    {
        Options.Verb = Verb.Run;
        ParseFlags(parseResult);

        string file = parseResult.GetValue(FileArg)!;

        CompileOutput output = Compiler.Compile(file);
        Error(output.Errors);

        _ = Process.Start(output.Executable).Start();

        return 0;
    }


    private static Command TestCommand()
    {
        Command testCommand = new("test", "Test compiler output");

        // Global Flags
        testCommand.Add(DebugFlag);
        testCommand.Add(TokensFlag);
        testCommand.Add(SymbolsFlag);

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
            Tester.RunTestFile(file);
        }
        else
        {
            Tester.Test("Tests/");
        }

        return 0;
    }



    private static void ParseFlags(ParseResult parseResult)
    {
        Options.Debug = parseResult.GetValue(DebugFlag);
        Options.Tokens = parseResult.GetValue(TokensFlag);
        Options.Symbols = parseResult.GetValue(SymbolsFlag);

        Options.UpdateSnapshots = parseResult.GetValue(UpdateSnapshotFlag);
    }
}
