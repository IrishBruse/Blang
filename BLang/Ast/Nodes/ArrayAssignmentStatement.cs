namespace BLang.Ast.Nodes;

using BLang.Utility;

public record ArrayAssignmentStatement(Symbol Symbol, Expression Index, Expression Value) : Statement
{
    public override string ToSource()
    {
        return $"{Symbol.Name}[{Index.ToSource()}] = {Value.ToSource()};";
    }
}
