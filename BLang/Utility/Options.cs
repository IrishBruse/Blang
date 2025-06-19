namespace BLang.Utility;

using System;
using CommandLine;
using CommandLine.Text;

public class Options
{
    public static string AppName => "bc";
    public static Version AppVersion => new(0, 1, 0);

    [Option('t', "target", HelpText = "The target to compile for.", Required = true)]
    public string Target { get; set; } = "";
    [Option("list-targets", HelpText = "List the supported compilation targets.")] public bool ListTargets { get; set; }

    [Option("run", HelpText = "Run the output executable")] public bool Run { get; set; }

    [Option("test", HelpText = "Run the tests.")] public bool Test { get; set; }
    [Option("update-snapshots", HelpText = "Update the test snapshots.")] public bool UpdateSnapshots { get; set; }

    [Option("debug", HelpText = "Print debug info about the compiler.")] public bool Debug { get; set; }
    [Option("tokens", HelpText = "Log each token as its read.")] public bool Tokens { get; set; }

    [Value(0, MetaName = "file", HelpText = "The b file to compile.")]
    public string File { get; set; } = "";

    public static void Parse(string[] args)
    {
        Parser parser = new((options) =>
        {
            options.AutoHelp = true;
            options.AutoVersion = true;
            options.EnableDashDash = true;
        });

        ParserResult<Options> parserResult = parser.ParseArguments<Options>(args);

        parserResult.WithParsed((o) => { options = o; }).WithNotParsed(errors =>
        {
            if (errors.IsVersion())
            {
                Console.WriteLine($"{AppName} v{AppVersion}");
            }
            else if (errors.IsHelp())
            {
                HelpText helpText = HelpText.AutoBuild(parserResult, h =>
                {
                    h.Heading = $"{AppName} v{AppVersion}";
                    h.Copyright = "Copyright (c) 2025 Ethan Conneely";
                    return HelpText.DefaultParsingErrorsHandler(parserResult, h);
                }, e => e);
                Console.WriteLine(helpText);
            }
        });

        if (options == null)
        {
            Environment.Exit(0);
        }

    }
}
