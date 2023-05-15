namespace IBlang;
public class Compiler
{
    public static void Run(string source)
    {
        StreamReader sourceFile = File.OpenText(source);

        Lexer lexer = new(sourceFile, true);
        Token[] tokens = lexer.Lex();

        Console.WriteLine();

        Parser parser = new(tokens);

        FileAst ast = parser.Parse();

        DebugNodeVisitor debugVisitor = new();
        debugVisitor.Visit(ast);
    }

    public static (string expected, string output) Test(string testFile)
    {
        string source = File.ReadAllText(testFile);

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

        Run(testFile);

        return (expected, expected);
    }
}
