using IBlang;

string[] examples = Directory.GetFiles(Path.GetFullPath("../Examples"), "*.ib");

foreach (string example in examples)
{
    Compiler.CompileAndRun(example);
}
