using IBlang;

var arguments = Environment.GetCommandLineArgs()[1..];
Console.WriteLine(string.Join(", ", arguments));

// Error tests
Compiler.Run("../Tests/Error.ib");
// Compiler.Run("../Tests/Syntax.ib");


// Compiler.Run("../Tests/HelloWorld.ib");
// Compiler.Run("../Tests/Fibonacci.ib");
// Compiler.Run("../Tests/syntax.ib");
