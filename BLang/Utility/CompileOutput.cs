namespace BLang.Utility;

using System.IO;
using BLang.Ast;

public class CompileOutput(string sourceFile, string executable, CompilationUnit compilationUnit, string[] errors)
{
    public string SourceFile { get; set; } = sourceFile;
    public string Executable { get; set; } = executable;
    public CompilationUnit CompilationUnit { get; set; } = compilationUnit;
    public string[] Errors { get; set; } = errors;

    public void WriteAst()
    {
        string astOutput = CompilationUnit.ToJson();
        string astFile = Path.ChangeExtension(SourceFile, "ast");
        File.WriteAllText(astFile, astOutput);
    }
}
