using System;
using System.IO;

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
        Print($"Test \"{Path.GetFileNameWithoutExtension(testFile)}\" has Passed!", ConsoleColor.Green);
    }
    else
    {
        Print($"Test \"{Path.GetFileNameWithoutExtension(testFile)}\" has Failed!", ConsoleColor.Red);
        Print($"Expected:", ConsoleColor.Gray);
        Print($"{expected}", ConsoleColor.Green);
        Print($"Got:", ConsoleColor.Gray);
        Print($"{output}", ConsoleColor.Red);
        Print("");
    }
}

Print($"{totalPassed}/{tests.Length} {(tests.Length > 1 ? "Tests" : "Test")} Passed");

static void Print(string text, ConsoleColor color = ConsoleColor.Gray)
{
    Console.ForegroundColor = color;
    Console.WriteLine(text);
    Console.ResetColor();
}
