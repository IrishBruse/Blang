namespace BLang.Tests;

using BLang.Utility;

public class UnitTest1
{
    private static string TestPath = "../../../../Tests/";

    [Theory]
    [MemberData(nameof(GetTests))]
    public void Test(string file)
    {
        BaseOptions.Parse(["test"]);

        string testFile = Path.GetFullPath(file);
        Tester.RunTestFile(testFile);
    }

    public static IEnumerable<object[]> GetTests()
    {
        return Tester.GetTests(TestPath);
    }

}
