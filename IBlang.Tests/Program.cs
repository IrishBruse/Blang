using IBlang;

string[] tests = Directory.GetFiles(Path.GetFullPath("./Tests/"), "*.ib");

int totalPassed = 0;

for (int i = 0; i < tests.Length; i++)
{
    string testFile = tests[i];

    (string output, string expected) = Compiler.Test(testFile);

    if (output == expected)
    {
        totalPassed++;
        Log.WriteLine($"Test \"{Path.GetFileNameWithoutExtension(testFile)}\" has Passed!", ConsoleColor.Green);
    }
    else
    {
        Log.WriteLine($"Test \"{Path.GetFileNameWithoutExtension(testFile)}\" has Failed!", ConsoleColor.Red);
        Log.WriteLine($"Expected:", ConsoleColor.Gray);
        Log.WriteLine($"{expected}", ConsoleColor.Green);
        Log.WriteLine($"Got:", ConsoleColor.Gray);
        Log.WriteLine($"{output}", ConsoleColor.Red);
        Log.WriteLine("");
    }
}

Console.WriteLine($"{totalPassed}/{tests.Length} {(tests.Length > 1 ? "Tests" : "Test")} Passed");
