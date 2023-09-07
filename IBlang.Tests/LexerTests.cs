namespace IBlang.Tests;

using Xunit;

public class LexerTests
{
    [Fact]
    public void LexEmpty()
    {
        Token[] tokens = Lex(string.Empty);

        Assert.Equal(TokenType.Eof, tokens[0].Type);
    }

    [Fact]
    public void LexDebug()
    {
        Lexer lexer = new("func", true);

        IEnumerator<Token> tokens = lexer.Lex();

        tokens.MoveNext();

        Assert.Equal(TokenType.Keyword_Func, tokens.Current.Type);
    }

    [Fact]
    public void LexComment()
    {
        Token[] tokens = Lex("// Comment");

        Assert.Equal(TokenType.Comment, tokens[0].Type);
    }

    [Fact]
    public void LexIdentifier()
    {
        Token[] tokens = Lex("word");

        Assert.Equal(TokenType.Identifier, tokens[0].Type);
    }

    [Fact]
    public void LexNumber()
    {
        Token[] tokens = Lex("420");

        Assert.Equal(TokenType.IntegerLiteral, tokens[0].Type);
    }

    [Fact]
    public void LexString()
    {
        Token[] tokens = Lex("\"Hello World\"");

        Assert.Equal(TokenType.StringLiteral, tokens[0].Type);
    }

    [Fact]
    public void LexWhitespace()
    {
        Token[] tokens = Lex(" \t\r\n");

        Assert.Equal(TokenType.Eof, tokens[0].Type);
    }

    [Fact]
    public void LexGarbage()
    {
        Token[] tokens = Lex("😫");

        Assert.Equal(TokenType.Garbage, tokens[0].Type);
    }

    [Fact]
    public void LexBrackets()
    {
        Token[] tokens = Lex("[] {} ()");

        TokenType[] expected = new TokenType[] {
            TokenType.OpenBracket, TokenType.CloseBracket,
            TokenType.OpenScope, TokenType.CloseScope,
            TokenType.OpenParenthesis, TokenType.CloseParenthesis,
            TokenType.Eof
        };

        Assert.Equal(expected, TestUtility.GetTokenTypes(tokens));
    }

    [InlineData("<=", TokenType.LessThanEqual)]
    [InlineData(">=", TokenType.GreaterThanEqual)]
    [InlineData("==", TokenType.EqualEqual)]
    [InlineData("!=", TokenType.NotEqual)]
    [InlineData("&&", TokenType.LogicalAnd)]
    [InlineData("||", TokenType.LogicalOr)]
    [InlineData("+=", TokenType.AdditionAssignment)]
    [InlineData("-=", TokenType.SubtractionAssignment)]
    [InlineData("*=", TokenType.MultiplicationAssignment)]
    [InlineData("/=", TokenType.DivisionAssignment)]
    [InlineData("%=", TokenType.ModuloAssignment)]
    [InlineData("<<", TokenType.BitwiseShiftLeft)]
    [InlineData(">>", TokenType.BitwiseShiftRight)]
    [Theory]
    public void LexBinaryOperators(string op, TokenType tokenType)
    {
        Token[] tokens = Lex(op);

        Assert.Equal(new TokenType[] { tokenType, TokenType.Eof }, TestUtility.GetTokenTypes(tokens));
    }

    private static Token[] Lex(string source)
    {
        using Lexer lexer = new(source);
        IEnumerable<Token> tokens = (IEnumerable<Token>)lexer.Lex();
        return tokens.ToArray();
    }
}
