namespace IBlang.Tests;

using Xunit;

public class CompilerTests
{
    [Theory]
    [InlineData("Empty")]
    public void Test1(string file)
    {
        file = $"Tests/{file}.ib";

        (string output, string expected) = Compiler.Test(file);

        Assert.Equal(output, expected);
    }
}
