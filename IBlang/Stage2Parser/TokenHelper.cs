namespace IBlang.Stage2Parser;

using IBlang.Stage1Lexer;

public static class TokenHelper
{
    public static bool IsArithmetic(TokenType tokenType)
    {
        return tokenType == TokenType.Addition ||
        tokenType == TokenType.Subtraction ||
        tokenType == TokenType.Multiplication ||
        tokenType == TokenType.Division ||
        tokenType == TokenType.Modulo;
    }

    public static bool IsRelational(TokenType tokenType)
    {
        return tokenType == TokenType.LessThan ||
        tokenType == TokenType.GreaterThan ||
        tokenType == TokenType.LessThanEqual ||
        tokenType == TokenType.GreaterThanEqual ||
        tokenType == TokenType.EqualEqual ||
        tokenType == TokenType.NotEqual;
    }

    public static bool IsLogical(TokenType tokenType)
    {
        return tokenType == TokenType.LogicalAnd ||
        tokenType == TokenType.LogicalOr ||
        tokenType == TokenType.LogicalNot;
    }

    public static bool IsBinaryToken(TokenType tokenType)
    {
        return IsArithmetic(tokenType) ||
        IsRelational(tokenType) ||
        IsLogical(tokenType);
    }

    public static bool IsAssignment(TokenType tokenType)
    {
        return tokenType == TokenType.Assignment ||
        tokenType == TokenType.AdditionAssignment ||
        tokenType == TokenType.SubtractionAssignment ||
        tokenType == TokenType.MultiplicationAssignment ||
        tokenType == TokenType.DivisionAssignment ||
        tokenType == TokenType.ModuloAssignment;
    }
}
