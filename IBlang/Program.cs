namespace IBlang;

using IBlang.LexerStage;
using IBlang.ParserStage;

public class Program
{
    public static void Main(string[] args)
    {
        string currentFile = @"../Examples/helloworld.ib";

        Lexer lexer = new(currentFile);
        Parser parser = new(lexer);

        parser.GetNextNode();
    }
}
