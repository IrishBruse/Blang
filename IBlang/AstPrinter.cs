namespace IBlang;

using System.Text;

public class AstPrinter : IAstVisitor
{
    int depth = 0;
    void Indent() => depth++;
    void Dedent() => depth--;
    string Space => new(' ', depth * 4);

    public string VisitCompilationUnit(CompilationUnit node)
    {
        StringBuilder sb = new();
        sb.AppendLine($"{Space}CompilationUnit:");

        Indent();
        foreach (FunctionDeclaration funcDecl in node.FunctionDeclarations)
        {
            sb.AppendLine(funcDecl.Accept(this));
        }

        foreach (FunctionDeclaration varDecl in node.VariableDeclarations)
        {
            sb.AppendLine(varDecl.Accept(this));
        }
        Dedent();
        return sb.ToString();
    }

    public string VisitFunctionDeclaration(FunctionDeclaration node)
    {
        StringBuilder sb = new();
        sb.AppendLine($"{Space}FunctionDeclaration:");

        Indent();
        foreach (Statement stmt in node.Statements)
        {
            sb.AppendLine(stmt.Accept(this));
        }
        Dedent();

        return sb.ToString();
    }

    public string VisitExternalStatement(ExternalStatement node)
    {
        return $"{Space}ExternalStatement: {node.ExternalName}";
    }

    public string VisitFunctionCall(FunctionCall node)
    {
        StringBuilder sb = new();

        Indent();
        foreach (Expression parameter in node.Parameters)
        {
            sb.Append(parameter.Accept(this));
        }
        Dedent();

        return $"{Space}FunctionCall: {node.IdentifierName} ({sb})";
    }

    public string VisitString(StringValue stringValue)
    {
        return stringValue.Value;
    }

    public string VisitInt(IntValue intValue)
    {
        return intValue.Value.ToString();
    }
}
