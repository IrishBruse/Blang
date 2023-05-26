namespace IBlang.Tests;

public class LexerTests
{
    [SetUp]
    public void Setup() { }

    [Test]
    public void LexEmpty()
    {
        Token[] tokens = Lex(string.Empty);

        Assert.That(tokens[0].Type, Is.EqualTo(TokenType.Eof));
    }

    [Test]
    public void LexDebug()
    {
        Lexer lexer = new("func", true);

        Token[] tokens = lexer.Lex();

        Assert.That(tokens[0].Type, Is.EqualTo(TokenType.Keyword_Func));
    }

    [Test]
    public void LexComment()
    {
        Token[] tokens = Lex("// Comment");

        Assert.That(tokens[0].Type, Is.EqualTo(TokenType.Comment));
    }

    [Test]
    public void LexIdentifier()
    {
        Token[] tokens = Lex("word");

        Assert.That(tokens[0].Type, Is.EqualTo(TokenType.Identifier));
    }

    [Test]
    public void LexNumber()
    {
        Token[] tokens = Lex("420");

        Assert.That(tokens[0].Type, Is.EqualTo(TokenType.IntegerLiteral));
    }

    [Test]
    public void LexString()
    {
        Token[] tokens = Lex("\"Hello World\"");

        Assert.That(tokens[0].Type, Is.EqualTo(TokenType.StringLiteral));
    }

    [Test]
    public void LexWhitespace()
    {
        Token[] tokens = Lex(" \t\r\n");

        Assert.That(tokens[0].Type, Is.EqualTo(TokenType.Eof));
    }

    [Test]
    public void LexGarbage()
    {
        Token[] tokens = Lex("😫");

        Assert.That(tokens[0].Type, Is.EqualTo(TokenType.Garbage));
    }

    [Test]
    public void LexBrackets()
    {
        Token[] tokens = Lex("[] {} ()");

        TokenType[] expected = new TokenType[] {
            TokenType.OpenBracket, TokenType.CloseBracket,
            TokenType.OpenScope, TokenType.CloseScope,
            TokenType.OpenParenthesis, TokenType.CloseParenthesis,
            TokenType.Eof
        };

        CollectionAssert.AreEqual(expected, TestUtility.GetTokenTypes(tokens));
    }

    [TestCase("<=", TokenType.LessThanEqual)]
    [TestCase(">=", TokenType.GreaterThanEqual)]
    [TestCase("==", TokenType.EqualEqual)]
    [TestCase("!=", TokenType.NotEqual)]
    [TestCase("&&", TokenType.LogicalAnd)]
    [TestCase("||", TokenType.LogicalOr)]
    [TestCase("+=", TokenType.AdditionAssignment)]
    [TestCase("-=", TokenType.SubtractionAssignment)]
    [TestCase("*=", TokenType.MultiplicationAssignment)]
    [TestCase("/=", TokenType.DivisionAssignment)]
    [TestCase("%=", TokenType.ModuloAssignment)]
    [TestCase("<<", TokenType.BitwiseShiftLeft)]
    [TestCase(">>", TokenType.BitwiseShiftRight)]
    public void LexBinaryOperators(string op, TokenType tokenType)
    {
        Token[] tokens = Lex(op);

        CollectionAssert.AreEqual(new TokenType[] { tokenType, TokenType.Eof }, TestUtility.GetTokenTypes(tokens));
    }

    private static Token[] Lex(string source)
    {
        using Lexer lexer = new(source);

        return lexer.Lex();
    }
}
