namespace BLang.Ast.Nodes;

using System.Text.Json.Serialization;
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
            return ret.Trim();
        }
        else
        {
            return $"({ret.Trim()})";
        }
    }
}

public record AddressOfExpression(Expression expr) : Expression
{
    public override string ToString()
    {
        return "&" + expr.ToString();
    }
}

public record PointerDereferenceExpression(Expression expr) : Expression
{
    public override string ToString()
    {
        return "*" + expr.ToString();
    }
}
