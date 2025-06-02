namespace BLang.Utility;

using CommandLine;

public class Flags
{
    public static Flags Instance { get; set; } = null!;

    [Option('t', "target", HelpText = "The target to compile for.", Required = true)]
    public string Target { get; set; } = "";

    [Option()]
    public bool Run { get; set; }
    [Option()]
    public bool Help { get; set; }
    [Option()]
    public bool ListTargets { get; set; }

    [Option()]
    public bool Test { get; set; }
    [Option()]
    public bool UpdateSnapshots { get; set; }

    [Option()]
    public bool Debug { get; set; }
    [Option()]
    public bool Tokens { get; set; }

    [Value(0, MetaName = "file", HelpText = "File.")]
    public string File { get; set; } = "";

    public static void Parse(string[] args)
    {
        Parser.Default.ParseArguments<Flags>(args)
              .WithParsed(flags =>
              {
                  Instance = flags;
              });

    }
}
