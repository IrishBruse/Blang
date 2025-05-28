namespace IBlang.Utility;

public static class Flags
{
    public static string Target { get; set; } = "";

    public static bool Help { get; set; }
    public static bool Run { get; set; }
    public static bool Debug { get; set; }
    public static bool ListTargets { get; set; }
    public static bool Test { get; set; }
    public static bool Ast { get; set; }
    public static bool UpdateSnapshots { get; set; }
}
