namespace BLang.Ast;

using BLang.Ast.Nodes;
using BLang.Utility;

public record FunctionDecleration(Symbol Symbol, Expression[] Parameters, Statement[] Body) : AstNode;
