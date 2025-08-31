namespace BLang.Utility;

using System;
using System.Collections.Generic;

using CommandLine;
using CommandLine.Text;

[Verb("test", HelpText = "Test Options.")]
public class TestOptions : Options
{
    [Option('u', "update", HelpText = "Update the test snapshots.")] public bool UpdateSnapshots { get; set; }
}

[Verb("build", isDefault: true, HelpText = "Build Options.")]
public class BuildOptions : Options
{
    [Value(0, MetaName = "file", HelpText = "The path to the b source file.", Required = true)]
    public string File { get; set; } = "";
}

[Verb("run", HelpText = "Run Options.")]
public class RunOptions : BuildOptions;

[Verb("format", HelpText = "Format Options.")]
public class FormatOptions : Options;

public class Options
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
            .WithParsed<BuildOptions>(opt => options = opt)
            .WithParsed<RunOptions>(opt => options = opt)
            .WithParsed<TestOptions>(opt => options = opt);

        parserResult.WithNotParsed(errors => HandleErrors(errors, parserResult));
    }

    static void HandleErrors(IEnumerable<Error> errors, ParserResult<object> parserResult)
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

            help.AddPreOptionsLine($"Usage: {AppName} <command> [options]");
            help.AddPreOptionsLine($"Usage: {AppName} <file> [options]");
            help.AddPreOptionsLine(" ");
            help.AddPreOptionsLine($"Options:");

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
