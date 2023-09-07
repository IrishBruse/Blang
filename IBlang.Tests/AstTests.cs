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

    private FileAst Parse(string file)
    {
        Lexer lexer = new(file);
        Tokens tokens = new(lexer.Lex());
        Parser parser = new(tokens, lexer.LineEndings);

        return parser.Parse();
    }
}
