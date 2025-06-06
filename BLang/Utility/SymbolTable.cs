namespace BLang.Utility;

using System;
using System.Collections.Generic;
using BLang.Tokenizer;

public record Symbol(string Name, SymbolKind Kind)
{
    public override string ToString() => Name;
}

public enum SymbolKind
{
    Variable,
    Function,
    Parameter,
    Global,
    External
}

// Represents the symbol table structure (managing scopes)
public class SymbolTable
{
    // A stack of dictionaries, where each dictionary represents a scope
    private readonly Stack<Dictionary<string, Symbol>> scopes;

    public SymbolTable()
    {
        scopes = new Stack<Dictionary<string, Symbol>>();
        EnterScope("global"); // Start with the global scope
    }

    public int CurrentScopeDepth { get; private set; } = -1;

    public void EnterScope(string name)
    {
        scopes.Push([]);
        CurrentScopeDepth++;
        Debug($"Entered scope {name} at depth {CurrentScopeDepth}", "SYM");
    }

    public void ExitScope()
    {
        if (scopes.Count > 1)
        {
            scopes.Pop();
            CurrentScopeDepth--;
            Debug($"Exited scope to depth: {CurrentScopeDepth}", "SYM");
        }
        else
        {
            throw new InvalidOperationException("Cannot exit global scope.");
        }
    }

    public Symbol Add(Token token, SymbolKind kind)
    {
        if (token.TokenType != TokenType.Identifier && !token.TokenType.IsKeyword())
        {
            throw new Exception(token.Content);
        }
        return Add(token.Content, kind);
    }

    public Symbol Add(string name, SymbolKind kind)
    {
        Symbol symbol = new(name, kind);
        Dictionary<string, Symbol> currentScope = scopes.Peek();
        if (currentScope.ContainsKey(name))
        {
            throw new InvalidOperationException($"Symbol '{name}' already declared in current scope.");
        }
        currentScope.Add(name, symbol);
        Debug($"Added {symbol.Kind} symbol: {symbol.Name} in scope {CurrentScopeDepth}", "SYM");

        return symbol;
    }

    public Symbol? Get(string name)
    {
        foreach (Dictionary<string, Symbol> scope in scopes)
        {
            if (scope.TryGetValue(name, out Symbol? symbol))
            {
                return symbol;
            }
        }
        return null;
    }

    public Symbol GetOrAdd(Token token, SymbolKind kind)
    {
        Symbol? symbol = Get(token.Content);

        if (symbol != null)
        {
            return symbol;
        }

        return Add(token, kind);
    }

    // Look up a symbol only in the current scope
    public Symbol? GetInCurrentScope(string name)
    {
        Dictionary<string, Symbol> currentScope = scopes.Peek();
        currentScope.TryGetValue(name, out Symbol? symbol);
        return symbol;
    }

    public void PrintSymbolTable()
    {
        Debug("\n--- Symbol Table Contents ---", "SYM");
        int depth = scopes.Count;
        foreach (Dictionary<string, Symbol> scope in scopes)
        {
            Debug($"Scope Depth: {depth--}", "SYM");
            foreach (KeyValuePair<string, Symbol> entry in scope)
            {
                Debug($"  {entry.Value}", "SYM");
            }
        }
    }
}
