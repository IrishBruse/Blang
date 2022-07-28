namespace IBlang.ParserStage;

using System.Runtime.CompilerServices;

using IBlang.LexerStage;

public partial class Parser
{
    private readonly Token[] tokens;
    private int currentTokenIndex;
    private Token PeekToken { get; set; }

    private void EatToken(TokenType expected, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string method = "")
    {
        Token got = NextToken();

        if (got.Type != expected)
        {
            Log.Error($"Expected '{string.Join(' ', expected)}' but got '{got}'", file, lineNumber, method);
        }
    }

    private string EatIdentifier([CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string method = "")
    {
        Token got = NextToken();
        const TokenType expected = TokenType.Identifier;
        if (got.Type != expected)
        {
            Log.Error($"Expected '{expected}' but got '{got}'", file, lineNumber, method);
        }
        return got.Value;
    }

    private ValueLiteral EatConstant()
    {
        string value = PeekToken.Value;

        TokenType got = NextToken().Type;

        switch (got)
        {
            case TokenType.StringLiteral: return new(ValueType.String, value);
            case TokenType.CharLiteral: return new(ValueType.Char, value);
            case TokenType.IntegerLiteral: return new(ValueType.Int, value);
            default: Log.Error($"Expected 'Identifier' but got '{got}'"); break;
        }

        return new(ValueType.String, value);
    }

    public Token NextToken()
    {
        Token token = PeekToken;
        Console.WriteLine("NextToken() = " + token);
        currentTokenIndex++;
        PeekToken = tokens[currentTokenIndex];
        return token;
    }

    private static bool IsArithmetic(TokenType tokenType) =>
        tokenType == TokenType.Addition ||
        tokenType == TokenType.Subtraction ||
        tokenType == TokenType.Multiplication ||
        tokenType == TokenType.Division ||
        tokenType == TokenType.Modulo;

    private static bool IsRelational(TokenType tokenType) =>
        tokenType == TokenType.LessThan ||
        tokenType == TokenType.GreaterThan ||
        tokenType == TokenType.LessThanEqual ||
        tokenType == TokenType.GreaterThanEqual ||
        tokenType == TokenType.EqualEqual ||
        tokenType == TokenType.NotEqual;

    private static bool IsLogical(TokenType tokenType) =>
        tokenType == TokenType.LogicalAnd ||
        tokenType == TokenType.LogicalOr ||
        tokenType == TokenType.LogicalNot;

    private static bool IsBinaryToken(TokenType tokenType) => IsArithmetic(tokenType) || IsRelational(tokenType) || IsLogical(tokenType);

    private static bool IsAssignment(TokenType tokenType) =>
        tokenType == TokenType.Assignment ||
        tokenType == TokenType.AdditionAssignment ||
        tokenType == TokenType.SubtractionAssignment ||
        tokenType == TokenType.MultiplicationAssignment ||
        tokenType == TokenType.DivisionAssignment ||
        tokenType == TokenType.ModuloAssignment;
}
