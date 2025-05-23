namespace IBlang;

using System.Collections.Generic;

public interface IAstVisitor
{
    string VisitCompilationUnit(CompilationUnit node);
    string VisitFunctionDeclaration(FunctionDeclaration node);
    string VisitExternalStatement(ExternalStatement node);
    string VisitFunctionCall(FunctionCall node);
    string VisitString(StringValue stringValue);
    string VisitInt(IntValue intValue);
}

public abstract record AstNode
{
    public required Range Range { get; init; }

    public abstract string Accept(IAstVisitor visitor);
}

public abstract record Statement() : AstNode;
public abstract record Expression() : AstNode;

public record StringValue(string Value) : Expression
{
    public override string Accept(IAstVisitor visitor) => visitor.VisitString(this);
}

public record IntValue(int Value) : Expression
{
    public override string Accept(IAstVisitor visitor) => visitor.VisitInt(this);
}

public record CompilationUnit(List<FunctionDeclaration> FunctionDeclarations, List<FunctionDeclaration> VariableDeclarations) : AstNode
{
    public override string Accept(IAstVisitor visitor) => visitor.VisitCompilationUnit(this);
}

public record FunctionDeclaration(Statement[] Statements) : AstNode
{
    public override string Accept(IAstVisitor visitor) => visitor.VisitFunctionDeclaration(this);
}

public record ExternalStatement(string ExternalName) : Statement
{
    public override string Accept(IAstVisitor visitor) => visitor.VisitExternalStatement(this);
}

public record FunctionCall(string IdentifierName, Expression[] Parameters) : Statement
{
    public override string Accept(IAstVisitor visitor) => visitor.VisitFunctionCall(this);
}
