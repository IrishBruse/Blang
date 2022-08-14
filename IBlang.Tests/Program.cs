using IBlang;

string[] tests = Directory.GetFiles(Path.GetFullPath("../Tests"), "*.ib");

foreach (string test in tests)
{
    bool passed = Compiler.Test(test);
    string result = passed ? "Passed" : "Failed";
    Console.WriteLine($"Test \"{test}\" has {result}!");
}
