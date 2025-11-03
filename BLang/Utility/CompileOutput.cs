namespace BLang.Utility;

using System.IO;
using BLang.Ast;

public class CompileOutput(string sourceFile)
{
    public bool Success { get; set; }

    public string SourceFile { get; set; } = sourceFile;
    public string? Executable { get; set; }
    public CompilationUnit? CompilationUnit { get; set; }
    public string? Error { get; set; }

    public void WriteAst()
    {
        if (CompilationUnit == null)
        {
            throw new ArgumentNullException(nameof(CompilationUnit));
        }
        string astOutput = CompilationUnit.ToJson();
        string astFile = Path.ChangeExtension(SourceFile, "ast");
        File.WriteAllText(astFile, astOutput);
    }
}
