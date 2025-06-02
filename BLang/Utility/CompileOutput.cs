namespace BLang.Utility;

public record struct CompileOutput(bool Success)
{
    public string Executable { get; set; } = "";
    public string Errors { get; set; } = "";
    public string AstOutput { get; set; } = "";
    public string? RunOutput { get; set; } = "";
}
