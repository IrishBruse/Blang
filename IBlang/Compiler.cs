namespace IBlang;

using IBlang.Data;
using IBlang.Walker;

public class Compiler
{
    public static void Run(string file)
    {
        Console.WriteLine($"\nCompiling {Path.GetFullPath(file)}");

        file = Path.GetFullPath(file);
        Project project = new();

        StreamReader sourceFile = File.OpenText(file);

        Console.WriteLine("\n-------- Lexer  --------");

        Lexer lexer = new(sourceFile, file, LexerDebug.Print);
        Tokens tokens = new(lexer.Lex(), lexer.LineEndings, true);
        Parser parser = new(tokens);
        FileAst ast = parser.Parse();

        Console.WriteLine("\n-------- Parser --------");

        AstVisitor debugVisitor = new(new PrintAstDebugger());
        debugVisitor.Visit(ast);
        tokens.ListErrors();

        Console.WriteLine("\n-------- TypeChecker --------");

        TypeChecker typeChecker = new(project);
        typeChecker.Check(ast);

        Console.WriteLine("\n-------- Transpiler --------");

        Transpiler transpiler = new(project);
        transpiler.TranspileToC(ast);
        transpiler.Compile(ast);
    }

    public static bool Test(string file)
    {
        string source = File.ReadAllText(file);

        string[] lines = source.Split("\n");

        string expected = "";

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            if (line.StartsWith("//"))
            {
                expected += line[2..] + "\n";
            }
        }

        Run(file);

        return expected.Equals(source);
    }
}
