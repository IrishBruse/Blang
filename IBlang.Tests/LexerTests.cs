namespace IBlang.Tests;

public class LexerTests
{
    [SetUp]
    public void Setup() { }

    [Test]
    public void LexEmpty()
    {
        using Lexer lexer = new(string.Empty);

        Token[] tokens = lexer.Lex();

        Assert.That(tokens[0].Type, Is.EqualTo(TokenType.Eof));
    }

    [Test]
    public void LexDebug()
    {
        using Lexer lexer = new("func", true);

        Token[] tokens = lexer.Lex();

        Assert.That(tokens[0].Type, Is.EqualTo(TokenType.Keyword_Func));
    }

    [Test]
    public void LexComment()
    {
        using Lexer lexer = new("// Comment");

        Token[] tokens = lexer.Lex();

        Assert.That(tokens[0].Type, Is.EqualTo(TokenType.Eof));
    }

    [Test]
    public void LexIdentifier()
    {
        using Lexer lexer = new("word");

        Token[] tokens = lexer.Lex();

        Assert.That(tokens[0].Type, Is.EqualTo(TokenType.Identifier));
    }

    [Test]
    public void LexNumber()
    {
        using Lexer lexer = new("420");

        Token[] tokens = lexer.Lex();

        Assert.That(tokens[0].Type, Is.EqualTo(TokenType.IntegerLiteral));
    }

    [Test]
    public void LexString()
    {
        using Lexer lexer = new("\"Hello World\"");

        Token[] tokens = lexer.Lex();

        Assert.That(tokens[0].Type, Is.EqualTo(TokenType.StringLiteral));
    }

    [Test]
    public void LexWhitespace()
    {
        using Lexer lexer = new(" \t\r\n");

        Token[] tokens = lexer.Lex();

        Assert.That(tokens[0].Type, Is.EqualTo(TokenType.Eof));
    }

    [Test]
    public void LexGarbage()
    {
        using Lexer lexer = new("😫");

        Token[] tokens = lexer.Lex();

        Assert.That(tokens[0].Type, Is.EqualTo(TokenType.Garbage));
    }

    [Test]
    public void LexBrackets()
    {
        using Lexer lexer = new("[] {} ()");

        Token[] tokens = lexer.Lex();

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
        using Lexer lexer = new(op);

        Token[] tokens = lexer.Lex();

        CollectionAssert.AreEqual(new TokenType[] { tokenType, TokenType.Eof }, TestUtility.GetTokenTypes(tokens));
    }


}
