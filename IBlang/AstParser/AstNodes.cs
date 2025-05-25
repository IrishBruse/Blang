namespace IBlang.AstParser;

using System.Collections.Generic;
using IBlang;

public interface IAstVisitor
{
    bool Output(CompilationUnit node, string file);

    void VisitCompilationUnit(CompilationUnit node);
    void VisitFunctionDeclaration(FunctionDeclaration node);
    void VisitExternalDeclaration(ExternalStatement node);
    void VisitFunctionCall(FunctionCall node);
    void VisitString(StringValue stringValue);
    void VisitInt(IntValue intValue);
}

public abstract record AstNode
{
    public required SourceRange Range { get; init; }

    public abstract void Accept(IAstVisitor visitor);
}

public abstract record Statement() : AstNode;
public abstract record Expression() : AstNode;

public record StringValue(string Value) : Expression
{
    public override void Accept(IAstVisitor visitor) => visitor.VisitString(this);
}

public record IntValue(int Value) : Expression
{
    public override void Accept(IAstVisitor visitor) => visitor.VisitInt(this);
}

public record CompilationUnit(List<FunctionDeclaration> FunctionDeclarations, List<FunctionDeclaration> VariableDeclarations) : AstNode
{
    public override void Accept(IAstVisitor visitor) => visitor.VisitCompilationUnit(this);
}

public record FunctionDeclaration(string FunctionName, Expression[] Parameters, Statement[] Statements) : AstNode
{
    public override void Accept(IAstVisitor visitor) => visitor.VisitFunctionDeclaration(this);
}

public record ExternalStatement(string ExternalName) : Statement
{
    public override void Accept(IAstVisitor visitor) => visitor.VisitExternalDeclaration(this);
}

public record FunctionCall(string IdentifierName, Expression[] Parameters) : Statement
{
    public override void Accept(IAstVisitor visitor) => visitor.VisitFunctionCall(this);
}
