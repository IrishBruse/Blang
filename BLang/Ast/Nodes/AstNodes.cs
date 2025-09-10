namespace BLang.Ast.Nodes;

using System.Collections.Generic;
using BLang.Utility;

public abstract record AstNode
{
#pragma warning disable CA1051
    public SourceRange Range = SourceRange.Zero;
#pragma warning restore CA1051
}

public record CompilationUnit(List<FunctionDecleration> FunctionDeclarations, List<GlobalVariable> GlobalVariables) : AstNode;

public record FunctionDecleration(Symbol Symbol, Expression[] Parameters, Statement[] Body) : AstNode;
