namespace IBlang.ParserStage;

using System.Runtime.CompilerServices;

using IBlang.LexerStage;

public partial class Parser
{
    private Token[] tokens;
    int currentTokenIndex;
    private Token PeekToken { get; set; }

    private void EatOptionalToken(TokenType expected, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
    {
        if (PeekToken.Type == expected)
        {
            EatToken(expected, file, lineNumber);
        }
    }

    private Token EatToken(TokenType expected, [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string method = "")
    {
        Token got = NextToken();
        if (got.Type != expected)
        {
            Log.Error($"Expected '{expected}' but got '{got}' in {method}()");
        }

        return got;
    }

    private string EatIdentifier()
    {
        string identifier = PeekToken.Value;

        TokenType got = NextToken().Type;
        const TokenType expected = TokenType.Identifier;
        if (got != expected)
        {
            Log.Error($"Expected '{expected}' but got '{got}'");
        }

        return identifier;
    }

    private string EatLiteral()
    {
        string identifier = PeekToken.Value;

        TokenType got = NextToken().Type;

        switch (got)
        {
            case TokenType.StringLiteral: return identifier;
            case TokenType.CharLiteral: return identifier;
            case TokenType.NumberLiteral: return identifier;
            default: Log.Error($"Expected 'Identifier' but got '{got}'"); break;
        }

        return identifier;
    }

    public Token NextToken()
    {
        var token = PeekToken;
        currentTokenIndex++;
        PeekToken = tokens[currentTokenIndex];
        return token;
    }
}
