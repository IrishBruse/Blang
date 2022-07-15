namespace IBlang.ParserStage;

using IBlang.LexerStage;

public class Parser
{
    private Lexer lexer;
    private IEnumerator<Token> tokens;

    public Parser(Lexer lexer)
    {
        this.lexer = lexer;
        tokens = lexer.GetNextToken().GetEnumerator();
    }

    public void GetNextNode()
    {
        foreach (Token token in lexer.GetNextToken())
        {
            if (token.Type == TokenType.Eof)
            {
                Console.WriteLine();
            }

            Console.Write(token + " ");

            if (token.Type == TokenType.Eol || token.Type == TokenType.Eof)
            {
                Console.WriteLine();
            }
        }
    }

    private Token NextToken()
    {
        _ = tokens.MoveNext();
        return tokens.Current;
    }
}
