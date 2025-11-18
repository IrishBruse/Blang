namespace BLang.Ast;

using System.Text.Json;
using System.Text.Json.Serialization;
using BLang.Ast.Nodes;

public record CompilationUnit(FunctionDecleration[] FunctionDeclarations, GlobalVariableDecleration[] GlobalVariables, GlobalArrayDeclaration[] GlobalArrays) : AstNode
{
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, CompilationUnitContext.Default.CompilationUnit);
    }

    public override string ToSource()
    {
        return "TODO:";
    }
}

[JsonSourceGenerationOptions(WriteIndented = true, IncludeFields = true)]
[JsonSerializable(typeof(CompilationUnit))]
public partial class CompilationUnitContext : JsonSerializerContext;
