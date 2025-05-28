namespace IBlang.Utility;

public record struct CompileOutput(bool Success)
{
#pragma warning disable CS8604
    public string Executable { get; set; } = "";
    public string Error { get; set; } = "";
    public string AstOutput { get; set; } = "";
    public string RunOutput { get; set; } = "";
#pragma warning restore CS8604
}
