namespace BLang.Ast.Nodes;

using System.Text.Json.Serialization;
using BLang.Ast;
using BLang.Tokenizer;
using BLang.Utility;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "_Type")]
[JsonDerivedType(typeof(StringValue), nameof(StringValue))]
[JsonDerivedType(typeof(IntValue), nameof(IntValue))]
[JsonDerivedType(typeof(Variable), nameof(Variable))]
[JsonDerivedType(typeof(BinaryExpression), nameof(BinaryExpression))]
[JsonDerivedType(typeof(AddressOfExpression), nameof(AddressOfExpression))]
[JsonDerivedType(typeof(PointerDereferenceExpression), nameof(PointerDereferenceExpression))]
public abstract record Expression() : AstNode;

public record StringValue(string Value) : Expression
{
    public override string ToString()
    {
        return Value.ToString();
    }
}

public record IntValue(int Value) : Expression
{
    public override string ToString()
    {
        return Value.ToString();
    }
}

public record Variable(Symbol Symbol) : Expression
{
    public override string ToString()
    {
        return Symbol.Name;
    }

    public static implicit operator Variable(Symbol d)
    {
        return new(d);
    }
}

public record BinaryExpression(BinaryOperator Operation, Expression Left, Expression Right) : Expression
{
    public override string ToString()
    {
        string ret = "";

        int parts = 0;

        if (Left != null)
        {
            ret += Left.ToString() + " ";
            parts++;
        }

        if (Operation != BinaryOperator.None)
        {
            ret += Operation.ToText() + " ";
            parts++;
        }

        if (Right != null)
        {
            ret += Right.ToString();
            parts++;
        }

        if (parts == 1)
        {
            return ret.Trim();
        }
        else
        {
            return $"({ret.Trim()})";
        }
    }
}

public record AddressOfExpression(Expression Expr) : Expression
{
    public override string ToString()
    {
        return "&" + Expr.ToString();
    }
}

public record PointerDereferenceExpression(Expression Expr) : Expression
{
    public override string ToString()
    {
        return "*" + Expr.ToString();
    }
}
