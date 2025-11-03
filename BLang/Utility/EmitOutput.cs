namespace BLang.Utility;

using BLang.Ast;

public class EmitOutput(string executable, CompilationUnit compilationUnit)
{
    public string Executable { get; set; } = executable;
    public CompilationUnit CompilationUnit { get; set; } = compilationUnit;
}
