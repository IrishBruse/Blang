namespace IBlang;
public class Compiler
{
    public static void Run(string source)
    {
        StreamReader sourceFile = File.OpenText(source);

        Lexer lexer = new(sourceFile);

        Token[] tokens = lexer.Lex();

        foreach (Token token in tokens)
        {
            // Console.WriteLine(token);
        }

        Parser parser = new(tokens);

        try
        {
            parser.Parse();
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(e.Message);
            Console.ResetColor();
        }
    }
}
