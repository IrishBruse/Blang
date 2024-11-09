using IBlang;

string[] arguments = Environment.GetCommandLineArgs()[1..];
Console.WriteLine(string.Join(", ", arguments));

// Error tests
// Compiler.Run("../Tests/Empty.ib");
// Compiler.Run("../Tests/Error.ib");
// Compiler.Run("../Tests/Syntax.ib");

int passed = 0;
int failed = 0;

_ = Compiler.Test("../Tests/PrintInt.ib") ? passed++ : failed++;
_ = Compiler.Test("../Tests/PrintString.ib") ? passed++ : failed++;

Console.WriteLine($"Total Tests: {passed + failed}. Passed: {passed}. Failed: {failed}");

// Compiler.Run("../Tests/HelloWorld.ib");
// Compiler.Run("../Tests/Fibonacci.ib", CompilationFlags.Print);
// Compiler.Run("../Tests/Syntax.ib");
