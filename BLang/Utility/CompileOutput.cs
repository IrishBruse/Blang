namespace BLang.Utility;

public struct CompileOutput()
{
    public bool Success { get; set; } = true;
    public string Executable { get; set; } = "";
    public string Errors { get; set; } = "";
    public string AstOutput { get; set; } = "";
}
