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
[JsonDerivedType(typeof(ArrayIndexExpression), nameof(ArrayIndexExpression))]
[JsonDerivedType(typeof(CallExpression), nameof(CallExpression))]
[JsonDerivedType(typeof(AssignmentExpression), nameof(AssignmentExpression))]
public abstract record Expression() : AstNode;

public record StringValue(string Value) : Expression
{
    public override string ToSource()
    {
        return '"' + Value + '"';
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}

public record IntValue(int Value) : Expression
{
    public override string ToSource()
    {
        return Value.ToString();
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}

public record Variable(Symbol Symbol) : Expression
{
    public override string ToSource()
    {
        return Symbol.Name;
    }

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
    public override string ToSource()
    {
        return ToString();
    }

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
    public override string ToSource()
    {
        return ToString();
    }

    public override string ToString()
    {
        return "&" + Expr.ToString();
    }
}

public record PointerDereferenceExpression(Expression Expr) : Expression
{
    public override string ToSource()
    {
        return ToString();
    }

    public override string ToString()
    {
        return "*" + Expr.ToString();
    }
}

public record ArrayIndexExpression(Variable Variable, Expression Index) : Expression
{
    public override string ToSource()
    {
        return ToString();
    }

    public override string ToString()
    {
        return Variable.ToString() + $"[{Index}]";
    }
}

public record CallExpression(Expression Callee, Expression[] Parameters) : Expression
{
    public override string ToSource()
    {
        return ToString();
    }

    public override string ToString()
    {
        string args = string.Join(", ", Parameters.Select(static p => p.ToString()));
        return $"{Callee}({args})";
    }
}

public record AssignmentExpression(BinaryOperator Operation, Expression Left, Expression Right) : Expression
{
    public override string ToSource()
    {
        return ToString();
    }

    public override string ToString()
    {
        if (Operation == BinaryOperator.None)
        {
            return $"({Left} = {Right})";
        }
        else
        {
            // Show compound assignment like `x += y` using the operation text
            return $"({Left} {Operation.ToText()}= {Right})";
        }
    }
}
