namespace IBlang.Data;
public record Token(string Value, TokenType Type, Span Span)
{
    public override string ToString()
    {
        return $"{Span} {Type} '{Value}'";
    }

    public bool IsBinaryOperator()
    {
        return Lexer.BinaryOperators.Contains(Type);
    }
}
