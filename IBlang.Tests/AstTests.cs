namespace IBlang.Tests;

using Xunit;

public class AstTests
{
    [Fact]
    public void ParseEmpty()
    {
        FileAst ast = Parse(string.Empty);
        Assert.Empty(ast.Functions);
    }

    static FileAst Parse(string file)
    {
        Lexer lexer = new();
        Project tokens = new(lexer.Lex(), lexer.LineEndings);
        Parser parser = new(tokens);

        return parser.Parse();
    }
}
