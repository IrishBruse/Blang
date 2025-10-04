namespace BLang.Targets;

using BLang.Ast;
using BLang.Utility;

public interface ITarget
{
    Result<CompileOutput> Emit(CompilationUnit unit, CompilerContext data);
}
