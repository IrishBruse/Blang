namespace BLang.Utility;

public record Symbol(string Name)
{
    public bool IsGlobal { get; set; }

    public override string ToString()
    {
        if (IsGlobal)
        {
            return "global_" + Name;
        }
        return Name;
    }
}
