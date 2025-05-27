namespace IBlang.AstParser;

using System.Collections.Generic;
using IBlang;

public interface ITargetVisitor
{
    void Output(CompilationUnit node, string file);

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

    public abstract void Accept(ITargetVisitor visitor);
}

public abstract record Statement() : AstNode;
public abstract record Expression() : AstNode;

public record StringValue(string Value) : Expression
{
    public override void Accept(ITargetVisitor visitor) => visitor.VisitString(this);
}

public record IntValue(int Value) : Expression
{
    public override void Accept(ITargetVisitor visitor) => visitor.VisitInt(this);
}

public record CompilationUnit(List<FunctionDeclaration> FunctionDeclarations, List<FunctionDeclaration> VariableDeclarations) : AstNode
{
    public override void Accept(ITargetVisitor visitor) => visitor.VisitCompilationUnit(this);
}

public record FunctionDeclaration(string FunctionName, Expression[] Parameters, Statement[] Statements) : AstNode
{
    public override void Accept(ITargetVisitor visitor) => visitor.VisitFunctionDeclaration(this);
}

public record ExternalStatement(string[] Externals) : Statement
{
    public override void Accept(ITargetVisitor visitor) => visitor.VisitExternalDeclaration(this);
}

public record FunctionCall(string IdentifierName, Expression[] Parameters) : Statement
{
    public override void Accept(ITargetVisitor visitor) => visitor.VisitFunctionCall(this);
}
