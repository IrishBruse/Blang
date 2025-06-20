namespace BLang.AstParser;

using System.Collections.Generic;
using BLang.Tokenizer;
using BLang.Utility;

public abstract record AstNode
{
    public SourceRange Range { get; set; } = SourceRange.Zero;
}

public record CompilationUnit(List<FunctionStatement> FunctionDeclarations, List<FunctionStatement> VariableDeclarations) : AstNode;

// Statements

public abstract record Statement() : AstNode;

public record FunctionStatement(Symbol Symbol, Expression[] Parameters, Statement[] Body) : Statement;

public record ExternalStatement(Symbol[] Externals) : Statement;

public record WhileStatement(BinaryExpression Condition, Statement[] Body) : Statement;

public record IfStatement(BinaryExpression Condition, Statement[] Body) : Statement;

public record AutoStatement(Symbol[] Variables) : Statement;

public record FunctionCall(Symbol Symbol, Expression[] Parameters) : Statement;

public record VariableDeclarator(Symbol Symbol, Expression Value) : Statement;

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

public record Variable(Symbol Symbol) : Expression
{
    public override string ToString() => Symbol.Name;
    public static implicit operator Variable(Symbol d) => new(d);
}

public record BinaryExpression(TokenType Operation, Expression Left, Expression Right) : Expression
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

        if (number == 1)
        {
            return $"{ret.Trim()}";
        }
        else
        {
            return $"({ret.Trim()})";
        }
    }
}

// Expression
