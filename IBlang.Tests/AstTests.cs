namespace IBlang.Tests;

public class AstTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void ParseEmpty()
    {
        using Lexer lexer = new(string.Empty);

        Token[] tokens = lexer.Lex();


    }
}
