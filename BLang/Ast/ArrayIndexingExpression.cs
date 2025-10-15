namespace BLang.Ast;

using BLang.Ast.Nodes;

public record ArrayIndexingExpression(Expression Target, Expression Index) : Expression;
