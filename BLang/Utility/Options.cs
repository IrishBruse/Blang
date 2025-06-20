namespace BLang.Utility;

using System;
using System.Linq;
using CommandLine;
using CommandLine.Text;

public enum Verb
{
    Run,
    Build,
    Test
}

[Verb("run", HelpText = "Run Options.")]
public class RunOptions : Options;

[Verb("test", HelpText = "Test Options.")]
public class TestOptions : Options;

[Verb("build", HelpText = "Build Options.")]
public class BuildOptions : Options;

public class Options
{
    public static string AppName => "bc";
    public static Version AppVersion => new(0, 1, 0);

    public Verb Verb { get; set; }

    [Option('t', "target", HelpText = "The target to compile for.")]
    public string Target { get; set; } = "qbe";

    [Option("list-targets", HelpText = "List the supported compilation targets.")]
    public bool ListTargets { get; set; }


    [Option("debug", HelpText = "Print debug info about the compiler.")] public bool Debug { get; set; }
    [Option("tokens", HelpText = "Log each token as its read.")] public bool Tokens { get; set; }

    [Value(0, MetaName = "file", HelpText = "The b file to compile.")]
    public string File { get; set; } = "";

    // Test options
    [Option("update-snapshots", HelpText = "Update the test snapshots.")] public bool UpdateSnapshots { get; set; }

    public static void Parse(string[] args)
    {
        Parser parser = new((options) =>
        {
            options.AutoHelp = true;
            options.AutoVersion = true;
            options.EnableDashDash = true;
        });

        ParserResult<object> parserResult = parser.ParseArguments<BuildOptions, RunOptions, TestOptions>(args);

        options = parserResult.MapResult<BuildOptions, RunOptions, TestOptions, Options>(
                 (BuildOptions opts) =>
                 {
                     opts.Verb = Verb.Build;
                     return opts;
                 },
                 (RunOptions opts) =>
                 {
                     opts.Verb = Verb.Run;
                     return opts;
                 },
                 (TestOptions opts) =>
                 {
                     opts.Verb = Verb.Test;
                     return opts;
                 },
                 errors =>
                 {
                     if (errors.IsVersion())
                     {
                         Console.WriteLine($"{AppName} v{AppVersion}");
                     }
                     else if (errors.IsHelp() || errors.Any())
                     {
                         HelpText helpText = HelpText.AutoBuild(parserResult, h =>
                         {
                             h.Heading = $"{AppName} v{AppVersion}";
                             h.Copyright = "Copyright (c) 2025 Ethan Conneely";
                             return HelpText.DefaultParsingErrorsHandler(parserResult, h);
                         }, e => e);
                         Console.WriteLine(helpText);
                     }
                     return null!;
                 }
             );

        if (options == null)
        {
            Environment.Exit(0);
        }
    }
}
