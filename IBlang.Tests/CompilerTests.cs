namespace IBlang.Tests;

public class CompilerTests
{
    [SetUp]
    public void Setup() { }

    [TestCase("Empty")]
    public void Test1(string file)
    {
        file = $"Tests/{file}.ib";

        (string output, string expected) = Compiler.Test(file);

        Assert.That(output, Is.EqualTo(expected));
    }
}
