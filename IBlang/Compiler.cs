namespace IBlang;
public class Compiler
{
    public static void Run(string source)
    {
        StreamReader sourceFile = System.IO.File.OpenText(source);

        Lexer lexer = new(sourceFile);

        Token[] tokens = lexer.Lex();

        // foreach (Token token in tokens)
        // {
        //     Console.WriteLine(token);
        // }

        Parser parser = new(tokens);

        try
        {
            parser.Parse();
        }
        catch (ParseException e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(e.Message);
            Console.Error.WriteLine(e.StackTrace);
            Console.ResetColor();
        }
    }

    public static (string output, string expected) Test(string testFile)
    {
        string source = System.IO.File.ReadAllText(testFile);

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
