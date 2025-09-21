namespace BLang.Ast;

using BLang.Ast.Nodes;

public record CompilationUnit(FunctionDecleration[] FunctionDeclarations, VariableDecleration[] GlobalVariables) : AstNode;
