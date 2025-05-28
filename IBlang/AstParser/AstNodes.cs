namespace IBlang.AstParser;

using System.Collections.Generic;
using IBlang;

public interface ITargetVisitor
{
    void Output(CompilationUnit node, string file);

    void VisitCompilationUnit(CompilationUnit node);
    void VisitFunctionDeclaration(FunctionStatement node);
    void VisitExternalDeclaration(ExternalStatement node);
    void VisitFunctionCall(FunctionCall node);
    void VisitString(StringValue stringValue);
    void VisitInt(IntValue intValue);
    void VisitAutoDeclaration(AutoStatement autoDeclaration);
    void VisitVariableAssignment(VariableAssignment variableAssignment);
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

public record CompilationUnit(List<FunctionStatement> FunctionDeclarations, List<FunctionStatement> VariableDeclarations) : AstNode
{
    public override void Accept(ITargetVisitor visitor) => visitor.VisitCompilationUnit(this);
}

public record FunctionStatement(string FunctionName, Expression[] Parameters, Statement[] Statements) : AstNode
{
    public override void Accept(ITargetVisitor visitor) => visitor.VisitFunctionDeclaration(this);
}

public record ExternalStatement(string[] Externals) : Statement
{
    public override void Accept(ITargetVisitor visitor) => visitor.VisitExternalDeclaration(this);
}

public record AutoStatement(string[] Variables) : Statement
{
    public override void Accept(ITargetVisitor visitor) => visitor.VisitAutoDeclaration(this);
}

public record FunctionCall(string IdentifierName, Expression[] Parameters) : Statement
{
    public override void Accept(ITargetVisitor visitor) => visitor.VisitFunctionCall(this);
}

public record VariableAssignment(string IdentifierName, string Value) : Statement
{
    public override void Accept(ITargetVisitor visitor) => visitor.VisitVariableAssignment(this);
}
