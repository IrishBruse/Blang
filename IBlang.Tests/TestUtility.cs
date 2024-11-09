namespace IBlang.Tests;

using IBlang.Data;

public static class TestUtility
{
    public static TokenType[] GetTokenTypes(Token[] tokens)
    {
        List<TokenType> tokenTypes = new();

        for (int i = 0; i < tokens.Length; i++)
        {
            tokenTypes.Add(tokens[i].Type);
        }

        return tokenTypes.ToArray();
    }


}
