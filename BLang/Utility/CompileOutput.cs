namespace BLang.Utility;

public class CompileOutput
{
    public bool Success { get; set; }

    public string? Executable { get; set; }
    public string Errors { get; set; } = "";
    public string AstOutput { get; set; } = "";
}
