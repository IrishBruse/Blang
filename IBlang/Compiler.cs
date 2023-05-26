namespace IBlang;
public class Compiler
{
    public static void Run(string file)
    {
        file = Path.GetFullPath(file);
        StreamReader sourceFile = File.OpenText(file);

        Lexer lexer = new(sourceFile, file, true, true);
        Token[] tokens = lexer.Lex();

        Parser parser = new(tokens, lexer.LineEndings);
        FileAst ast = parser.Parse();

        parser.ListErrors();

        if (!parser.HasErrors)
        {
            DebugNodeVisitor debugVisitor = new();
            debugVisitor.Visit(ast);
        }
    }

    public static (string expected, string output) Test(string file)
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

        return (expected, expected);
    }
}
