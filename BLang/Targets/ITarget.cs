namespace BLang.Targets;

using BLang.Ast;
using BLang.Utility;

public interface ITarget
{
    Result<EmitOutput> Emit(CompilationUnit unit, CompilerContext data);
}
