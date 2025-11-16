namespace BLang.Utility;

using BLang.Exceptions;
using BLang.Tokenizer;

public class SymbolTable
{
    private readonly Stack<Dictionary<string, Symbol>> scopes;
    private readonly Stack<string> scopeNames;

    public SymbolTable()
    {
        scopes = new Stack<Dictionary<string, Symbol>>();
        scopeNames = new Stack<string>();
    }

    public int CurrentScopeDepth { get; private set; }

    public void EnterScope(string name)
    {
        scopes.Push(new());
        scopeNames.Push(name);

        Log($"{name}:");
        CurrentScopeDepth++;
    }

    public void ExitScope()
    {
        _ = scopes.Pop();
        _ = scopeNames.Pop();
        CurrentScopeDepth--;
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
        Symbol symbol = new(name);
        Dictionary<string, Symbol> currentScope = scopes.Peek();
        if (currentScope.ContainsKey(name))
        {
            // Is this how shadowing works would i return a new symbol here
            // or maybe it should work like a stack
            throw new InvalidOperationException($"Symbol '{name}' already declared in current scope.");
        }
        currentScope.Add(name, symbol);
        Log($"{symbol.Name}");

        return symbol;
    }

    public Symbol GetOrAdd(Token token)
    {
        foreach (Dictionary<string, Symbol> scope in scopes)
        {
            if (scope.TryGetValue(token.Content, out Symbol? symbol))
            {
                return symbol;
            }
        }

        return Add(token);
    }

    public void Log(string message)
    {
        if (Options.Symbols && Options.Verbose > 1)
        {
            Globals.Log(new string(' ', CurrentScopeDepth * 2) + message, null, ConsoleColor.DarkGray);
        }
    }

    public void Clear()
    {
        scopes.Clear();
        scopeNames.Clear();

        scopes.Push(new());
        scopeNames.Push("global");

        CurrentScopeDepth = 0;
    }
}
