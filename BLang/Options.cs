namespace BLang;

public static class Options
{
    public static Verb Verb { get; set; }

    // Global Flags
    public static bool Debug { get; set; }
    public static bool Tokens { get; set; }
    public static bool Symbols { get; set; }


    // Run Flags

    // Test Flags
    public static bool UpdateSnapshots { get; set; }

    // Build Flags
}

public enum Verb
{
    Run,
    Build,
    Test,
}
