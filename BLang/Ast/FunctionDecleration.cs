namespace BLang.Ast;

using BLang.Ast.Nodes;
using BLang.Utility;

public record FunctionDecleration(Symbol Symbol, Variable[] Parameters, Statement[] Body) : AstNode
{
    public override string ToSource()
    {
        return $"TODO: ";
    }
}
