namespace BLang.Utility;

using System.Collections.Generic;

public class SymbolTable
{
    public SymbolTable()
    {
        EnterScope();
    }

    readonly Stack<Scope> scopeStack = new();
    int nextScopeId = 0;


    public void EnterScope()
    {
        Debug($"Entered Scope {nextScopeId}", "SYM");

        var newScope = new Scope(nextScopeId);
        scopeStack.Push(newScope);
        nextScopeId++;
    }

    public void ExitScope()
    {
        if (options.Debug)
        {
            PrintAllScopes();
        }

        Debug($"Exiting Scope {scopeStack.Peek().ScopeId}", "SYM");
        scopeStack.Pop();
    }

    public bool AddSymbol(string name, string type, object? value = null)
    {
        if (scopeStack.Count == 0)
        {
            Debug("No active scope to add symbol to.", "ERROR");
            return false;
        }

        Scope currentScope = scopeStack.Peek();
        SymbolInfo symbol = new(name, type, value);

        if (currentScope.TryAddSymbol(symbol))
        {
            Debug($"Added symbol '{name}' to Scope {currentScope.ScopeId}", "SYM");
            return true;
        }
        else
        {
            Debug($"Symbol '{name}' already declared in current Scope {currentScope.ScopeId}.", "ERROR");
            return false;
        }
    }


    public SymbolInfo? LookupSymbol(string name)
    {
        Debug($"Looking up symbol '{name}'...", "SYM");
        foreach (Scope scope in scopeStack)
        {
            SymbolInfo? symbol = scope.GetSymbol(name);
            if (symbol != null)
            {
                Debug($"Found '{name}' in Scope {scope.ScopeId}.", "SYM");
                return symbol;
            }
        }
        Debug($"Symbol '{name}' not found in any active scope.", "ERROR");
        return null;
    }

    public void PrintAllScopes()
    {
        Debug("\n=== Current Symbol Table State ===");
        if (scopeStack.Count == 0)
        {
            Debug("No scopes active.");
            return;
        }

        Scope[] scopesArray = scopeStack.ToArray();
        for (int i = 0; i < scopesArray.Length; i++)
        {
            scopesArray[i].PrintScopeContents();
        }
        Debug("==================================\n");
    }
}

public record SymbolInfo(string Identifier, string type, object? value = null);

public class Scope(int scopeId)
{
    public int ScopeId { get; } = scopeId;

    private readonly Dictionary<string, SymbolInfo> symbols = [];

    public bool TryAddSymbol(SymbolInfo symbol)
    {
        if (symbols.ContainsKey(symbol.Identifier))
        {
            return false;
        }
        symbols.Add(symbol.Identifier, symbol);
        return true;
    }

    public SymbolInfo? GetSymbol(string name)
    {
        symbols.TryGetValue(name, out SymbolInfo? symbol);
        return symbol;
    }

    public void PrintScopeContents()
    {
        Debug($"--- Scope ({ScopeId}) Contents ---");
        if (symbols.Count == 0)
        {
            Debug("    (Empty)");
        }

        foreach ((_, SymbolInfo? Value) in symbols)
        {
            Debug($"    {Value}");
        }
    }
}
