namespace BLang.Ast.Nodes;

using BLang.Utility;

public record ArrayAssignmentStatement(Symbol Symbol, Expression Index, Expression Value) : Statement;
