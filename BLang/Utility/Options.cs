namespace BLang.Utility;

using System;
using System.Collections.Generic;

using CommandLine;
using CommandLine.Text;

[Verb("test", HelpText = "Test Options.")]
public class TestOptions : BaseOptions
{
    [Option('u', "update", HelpText = "Update the test snapshots.")] public bool UpdateSnapshots { get; set; }
}

[Verb("build", isDefault: true, HelpText = "Build Options.")]
public class BuildOptions : BaseOptions
{
    [Value(0, MetaName = "file", HelpText = "The path to the b source file.", Required = true)]
    public string File { get; set; } = "";
}

[Verb("run", HelpText = "Run Options.")]
public class RunOptions : BuildOptions;

[Verb("format", HelpText = "Format Options.")]
public class FormatOptions : BaseOptions;

public class BaseOptions
{
    public static string AppName => "bc";
    public static Version AppVersion => new(0, 1, 0);

    [Option('t', "target", HelpText = "The target to compile for.")]
    public string Target { get; set; } = "qbe";

    [Option("list-targets", HelpText = "List the supported compilation targets.")]
    public bool ListTargets { get; set; }

    [Option("debug", HelpText = "Print debug info about current compilation.")] public bool Debug { get; set; }
    [Option("debug-symbols", HelpText = "Print debug info about current compilation.")] public bool DebugSymbol { get; set; }
    [Option("tokens", HelpText = "Log each token as its read.")] public bool Tokens { get; set; }

    public static void Parse(string[] args)
    {
        Parser parser = new((options) =>
        {
            options.AutoHelp = true;
            options.AutoVersion = true;
            options.EnableDashDash = true;
            options.HelpWriter = null;
        });

        ParserResult<object> parserResult = parser.ParseArguments<BuildOptions, RunOptions, TestOptions>(args)
            .WithParsed<BuildOptions>(opt => Options = opt)
            .WithParsed<RunOptions>(opt => Options = opt)
            .WithParsed<TestOptions>(opt => Options = opt);

        _ = parserResult.WithNotParsed(errors => HandleErrors(errors, parserResult));
    }

    private static void HandleErrors(IEnumerable<Error> errors, ParserResult<object> parserResult)
    {
        if (errors.IsVersion())
        {
            Console.Out.WriteLine($"{AppName} v{AppVersion}");
            Environment.Exit(0);
        }

        HelpText text = HelpText.AutoBuild(parserResult, help =>
        {
            help.Heading = "";
            help.Copyright = "";
            help.AdditionalNewLineAfterOption = false;
            help.AddDashesToOption = true;

            _ = help.AddPreOptionsLine($"Usage: {AppName} <command> [options]");
            _ = help.AddPreOptionsLine($"Usage: {AppName} <file> [options]");
            _ = help.AddPreOptionsLine(" ");
            _ = help.AddPreOptionsLine($"Options:");

            return help;
        }, example =>
        {
            Error("Example " + example);
            return example;
        }, true);

        Error(text);

        Environment.Exit(1);
    }
}
