namespace BLang;

using System.CommandLine;
using System.Diagnostics;
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

    private static Option<bool> AstFlag = new("--ast")
    {
        Description = "Dump compiler ast information",
    };

    private static Option<bool> TokensFlag = new("--tokens")
    {
        Description = "Print tokenizers tokens",
    };

    private static Option<bool> SymbolsFlag = new("--symbols")
    {
        Description = "Print symbol table",
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

        Result<CompileOutput> output = Compiler.Compile(file);

        if (!output)
        {
            return -1;
        }

        if (Options.Ast)
        {
            output.Value.WriteAst();
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

        Result<CompileOutput> output = Compiler.Compile(file);

        int code = 0;
        output.Success((output) =>
        {
            Process.Start(output.Executable).WaitForExit();
        }).Failure((error) =>
        {
            Error(error);
            code = 1;
        });

        return code;
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
            Tester.RunTestFile(file);
        }
        else
        {
            Options.Ast = true;
            Tester.Test("Tests/");

            Options.Ast = false;
            Tester.Test("Examples/");
        }

        return 0;
    }

    private static void GlobalFlags(Command rootCommand)
    {
        rootCommand.Add(TokensFlag);
        rootCommand.Add(SymbolsFlag);
        rootCommand.Add(AstFlag);

        rootCommand.Add(VerboseFlag);
        rootCommand.Add(VeryVerboseFlag);
    }

    private static void ParseFlags(ParseResult parseResult)
    {
        int verboseLevel = 0;

        verboseLevel = parseResult.GetValue(VerboseFlag) ? 1 : verboseLevel;
        verboseLevel = parseResult.GetValue(VeryVerboseFlag) ? 2 : verboseLevel;

        Options.Verbose = verboseLevel;
        Options.Ast = parseResult.GetValue(AstFlag);
        Options.Tokens = parseResult.GetValue(TokensFlag);
        Options.Symbols = parseResult.GetValue(SymbolsFlag);

        Options.UpdateSnapshots = parseResult.GetValue(UpdateSnapshotFlag);
    }
}
