namespace BLang.Ast;

using System.Text.Json;
using System.Text.Json.Serialization;
using BLang.Ast.Nodes;

public record CompilationUnit(FunctionDecleration[] FunctionDeclarations, VariableDeclaration[] GlobalVariables) : AstNode
{
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, CompilationUnitContext.Default.CompilationUnit);
    }
}

[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
[JsonSerializable(typeof(CompilationUnit))]
public partial class CompilationUnitContext : JsonSerializerContext;
