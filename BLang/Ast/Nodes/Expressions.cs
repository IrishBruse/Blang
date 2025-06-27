namespace BLang.Ast.Nodes;

using BLang.Tokenizer;
using BLang.Utility;

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
            return ret.Trim();
        }
        else
        {
            return $"({ret.Trim()})";
        }
    }
}
