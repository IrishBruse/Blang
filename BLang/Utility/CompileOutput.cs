namespace BLang.Utility;

public struct CompileOutput()
{
    public int ExitCode { get; set; }
    public string Executable { get; set; } = "";
    // public string Errors { get; set; } = "";
    // public string AstOutput { get; set; } = "";
}
