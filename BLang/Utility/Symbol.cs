namespace BLang.Utility;

public record Symbol(string Name, SymbolKind Kind = SymbolKind.Load)
{
    public override string ToString()
    {
        return Name;
    }
}
