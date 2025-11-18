namespace BLang.Utility;

using BLang.Exceptions;
using BLang.Tokenizer;

public class SymbolTable
{
    private readonly Dictionary<string, Symbol> symbols;

    public SymbolTable()
    {
        symbols = new();
    }

    public Symbol Add(Token token)
    {
        if (token.TokenType != TokenType.Identifier && !token.TokenType.IsKeyword())
        {
            throw new ParserException(token.Content);
        }
        return Add(token.Content);
    }

    public Symbol Add(string name)
    {
        if (symbols.TryGetValue(name, out Symbol? symbol))
        {
            return symbol;
        }

        symbol = new(name);
        symbols.Add(name, symbol);
        return symbol;

        // Symbol symbol = new(name);
        // if (symbols.ContainsKey(name))
        // {
        //     throw new InvalidOperationException($"Symbol '{name}' already declared in current scope.");
        // }
        // symbols.Add(name, symbol);

        // return symbol;
    }

    public Symbol GetOrAdd(Token token)
    {
        if (symbols.TryGetValue(token.Content, out Symbol? symbol))
        {
            return symbol;
        }

        return Add(token);
    }
}
