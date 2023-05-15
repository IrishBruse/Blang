namespace IBlang.Tests;

public class AstTests
{
    [SetUp]
    public void Setup() { }

    [Test]
    public void ParseEmpty()
    {
        FileAst ast = Parse(string.Empty);
        Assert.That(ast.Functions, Is.Empty);
    }

    private FileAst Parse(string file)
    {
        Lexer lexer = new(file);
        Token[] tokens = lexer.Lex();
        Parser parser = new(tokens);

        return parser.Parse();
    }
}
