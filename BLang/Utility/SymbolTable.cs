namespace BLang.Utility;

using System;
using System.Collections.Generic;
using BLang.Tokenizer;

public record Symbol(string Name, SymbolKind Kind = SymbolKind.Load)
{
    public override string ToString() => Name;
}

public enum SymbolKind
{
    Define, Assign, Load
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
        Log($"Entered scope {name} at depth {CurrentScopeDepth}");
    }

    public void ExitScope()
    {
        if (scopes.Count > 1)
        {
            scopes.Pop();
            CurrentScopeDepth--;
            Log($"Exited scope to depth: {CurrentScopeDepth}");
        }
        else
        {
            throw new InvalidOperationException("Cannot exit global scope.");
        }
    }

    public Symbol Add(Token token, SymbolKind kind = SymbolKind.Load)
    {
        if (token.TokenType != TokenType.Identifier && !token.TokenType.IsKeyword())
        {
            throw new Exception(token.Content);
        }
        return Add(token.Content, kind);
    }

    public Symbol Add(string name, SymbolKind kind = SymbolKind.Load)
    {
        Symbol symbol = new(name, kind);
        Dictionary<string, Symbol> currentScope = scopes.Peek();
        if (currentScope.ContainsKey(name))
        {
            // Is this how shadowing works would i return a new symbol here
            // or maybe it should work like a stack
            throw new InvalidOperationException($"Symbol '{name}' already declared in current scope.");
        }
        currentScope.Add(name, symbol);
        Log($"Added {symbol.Kind} symbol: {symbol.Name} in scope {CurrentScopeDepth}");

        return symbol;
    }

    public Symbol Get(string name)
    {
        foreach (Dictionary<string, Symbol> scope in scopes)
        {
            if (scope.TryGetValue(name, out Symbol? symbol))
            {
                return symbol;
            }
        }

        throw new Exception($"Couldnt find symbol \"{name}\" in any scope");
    }

    public Symbol GetOrAdd(Token token, SymbolKind kind = SymbolKind.Load)
    {
        foreach (Dictionary<string, Symbol> scope in scopes)
        {
            if (scope.TryGetValue(token.Content, out Symbol? symbol))
            {
                return symbol;
            }
        }

        return Add(token, kind);
    }

    /// <summary> Look up a symbol only in the current scope </summary>
    public Symbol? GetInCurrentScope(string name)
    {
        Dictionary<string, Symbol> currentScope = scopes.Peek();
        currentScope.TryGetValue(name, out Symbol? symbol);
        return symbol;
    }

    public void PrintSymbolTable()
    {
        Log("\n--- Symbol Table Contents ---");
        int depth = scopes.Count;
        foreach (Dictionary<string, Symbol> scope in scopes)
        {
            Log($"Scope Depth: {depth--}");
            foreach (KeyValuePair<string, Symbol> entry in scope)
            {
                Log($"  {entry.Value}");
            }
        }
    }

    public static void Log(string message)
    {
        if (options.DebugSymbol)
        {
            Debug(message, "SYM");
        }
    }
}
