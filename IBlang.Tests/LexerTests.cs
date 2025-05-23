namespace IBlang.Tests;


using Xunit;

public class LexerTests
{
    readonly Lexer lexer = new(CompilationFlags.None);

    [Fact]
    public void LexEmpty()
    {
        Token[] tokens = Lex(string.Empty);

        Assert.Equal(TokenType.Eof, tokens[0].Type);
    }
}
