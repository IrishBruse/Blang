namespace BLang.Ast.Nodes;

using System.Collections.Generic;
using BLang.Utility;

public abstract record AstNode
{
    public SourceRange Range { get; set; } = SourceRange.Zero;
}

public record CompilationUnit(List<FunctionDecleration> FunctionDeclarations, List<GlobalVariable> GlobalVariables) : AstNode;

public record FunctionDecleration(Symbol Symbol, Expression[] Parameters, Statement[] Body) : AstNode;
