namespace BLang;

using System.CommandLine;
using System.Diagnostics;
using System.IO;
using BLang.Utility;

public class Program
{
    private static Option<bool> DebugFlag = new("--debug")
    {
        Description = "Print compiler debug information",
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

    private static Argument<string> FileArg = new("file")
    {
        Description = "Path to b file",
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

        rootCommand.Add(FileArg);

        GlobalFlags(rootCommand);

        rootCommand.SetAction(Build);
        return rootCommand;
    }

    private static int Build(ParseResult parseResult)
    {
        Options.Verb = Verb.Build;
        ParseFlags(parseResult);

        string file = parseResult.GetValue(FileArg)!;

        if (!Compiler.TryCompile(file, out CompileOutput? output))
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
        runCommand.Add(FileArg);

        runCommand.SetAction(Run);
        return runCommand;
    }


    private static int Run(ParseResult parseResult)
    {
        Options.Verb = Verb.Run;
        ParseFlags(parseResult);

        string file = parseResult.GetValue(FileArg)!;


        if (!Compiler.TryCompile(file, out CompileOutput? output))
        {
            return 1;
        }

        if (output.Executable != null)
        {
            _ = Process.Start(output.Executable);
        }

        return 0;
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
            Tester.Test("Tests/");
        }

        return 0;
    }

    private static void GlobalFlags(Command rootCommand)
    {
        rootCommand.Add(TokensFlag);
        rootCommand.Add(SymbolsFlag);
        rootCommand.Add(AstFlag);

        rootCommand.Add(DebugFlag);
    }

    private static void ParseFlags(ParseResult parseResult)
    {
        Options.Debug = parseResult.GetValue(DebugFlag);
        Options.Ast = parseResult.GetValue(AstFlag);
        Options.Tokens = parseResult.GetValue(TokensFlag);
        Options.Symbols = parseResult.GetValue(SymbolsFlag);

        Options.UpdateSnapshots = parseResult.GetValue(UpdateSnapshotFlag);
    }
}
