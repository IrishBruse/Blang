namespace BLang.AstParser;

using System.Collections.Generic;
using BLang;
using BLang.Tokenizer;

public abstract record AstNode
{
    public required SourceRange Range { get; init; }
}

public record CompilationUnit(List<FunctionStatement> FunctionDeclarations, List<FunctionStatement> VariableDeclarations) : AstNode
{
    public string File { get; set; } = "";
}

// Statements

public abstract record Statement() : AstNode;

public record FunctionStatement(string FunctionName, Expression[] Parameters, Statement[] Body) : Statement;

public record ExternalStatement(string[] Externals) : Statement;

public record WhileStatement(BinaryExpression Condition, Statement[] Body) : Statement;

public record AutoStatement(string[] Variables) : Statement;

public record FunctionCall(string IdentifierName, Expression[] Parameters) : Statement;

public record VariableAssignment(string IdentifierName, BinaryExpression Value) : Statement;

// Statements

// Expression

public abstract record Expression() : AstNode;

public record StringValue(string Value) : Expression
{
    public override string ToString() => Value.ToString();
}

public record IntValue(int Value) : Expression
{
    public override string ToString() => Value.ToString();
}

public record Variable(string Identifier) : Expression
{
    public override string ToString() => Identifier.ToString();
}

public record BinaryExpression(TokenType Operation, Expression? Left, Expression? Right) : Expression
{
    public override string ToString()
    {
        string ret = "";

        int number = 0;

        if (Left != null)
        {
            ret += Left + " ";
            number++;
        }

        if (Operation != TokenType.None)
        {
            ret += Operation.ToCharString() + " ";
            number++;
        }

        if (Right != null)
        {
            ret += Right;
            number++;
        }

        if (number > 1)
        {
            return $"({ret.Trim()})";
        }
        else
        {
            return $"{ret.Trim()}";
        }
    }
}

// Expression
