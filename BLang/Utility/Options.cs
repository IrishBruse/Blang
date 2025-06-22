namespace BLang.Utility;

using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using CommandLine.Text;

[Verb("test", HelpText = "Test Options.")]
public class TestOptions : Options
{
    [Option("update-snapshots", HelpText = "Update the test snapshots.")] public bool UpdateSnapshots { get; set; }
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

    [Option("debug", HelpText = "Print debug info about the compiler.")] public bool Debug { get; set; }
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

        ParserResult<object> res = parser.ParseArguments<BuildOptions, RunOptions, TestOptions>(args)
              .WithParsed<BuildOptions>(opt =>
              {
                  options = opt;
              })
              .WithParsed<RunOptions>(opt =>
              {
                  options = opt;
              })
              .WithParsed<TestOptions>(opt =>
              {
                  options = opt;
              });

        res.WithNotParsed(errors =>
        {
            foreach (Error? error in errors)
            {
                Console.WriteLine($"Error: {error.Tag}");
            }

            HelpText helpText = HelpText.AutoBuild(res, h =>
            {
                h.Heading = $"{AppName} v{AppVersion}";
                h.Copyright = string.Empty;
                return HelpText.DefaultParsingErrorsHandler(res, h);
            }, e => e);
            Console.WriteLine(helpText);

        });

        // Parser parser = new((options) =>
        // {
        //     options.AutoHelp = true;
        //     options.AutoVersion = true;
        //     options.EnableDashDash = true;
        // });

        // ParserResult<object> parserResult = parser.ParseArguments<BuildOptions, RunOptions, TestOptions>(args);

        // options = parserResult.MapResult<BuildOptions, RunOptions, TestOptions, Options>(
        //         (BuildOptions opts) =>
        //         {
        //             opts.Verb = Verb.Build;
        //             return opts;
        //         },
        //         (RunOptions opts) =>
        //         {
        //             opts.Verb = Verb.Run;
        //             return opts;
        //         },
        //         (TestOptions opts) =>
        //         {
        //             opts.Verb = Verb.Test;
        //             return opts;
        //         },
        //         errors =>
        //         {
        //             if (errors.Count() > 1)
        //             {
        //                 throw new Exception("Unexpected error: more than 1 error returned from options parsing");
        //             }

        //             Console.WriteLine(errors.First());

        //             if (errors.IsVersion())
        //             {
        //                 Console.WriteLine($"{AppName} v{AppVersion}");
        //             }

        //             if (errors.IsHelp() || errors.IsError(ErrorType.NoVerbSelectedError))
        //             {
        //                 HelpText helpText = HelpText.AutoBuild(parserResult, h =>
        //                 {
        //                     h.Heading = $"{AppName} v{AppVersion}";
        //                     h.Copyright = string.Empty;
        //                     return HelpText.DefaultParsingErrorsHandler(parserResult, h);
        //                 }, e => e);
        //                 Console.WriteLine(helpText);
        //             }

        //             return null!;
        //         }
        //     );

        if (options == null)
        {
            Environment.Exit(0);
        }
    }
}

public static class HelpTextExtensions
{
    public static bool IsError(this IEnumerable<Error> errs, ErrorType error)
    {
        if (errs.Any((Error x) => x.Tag == error))
        {
            return true;
        }
        return false;
    }
}
