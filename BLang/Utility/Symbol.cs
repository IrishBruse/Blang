namespace BLang.Utility;

public record Symbol(string Name)
{
    public bool IsGlobal { get; set; }

    // public override string ToString()
    // {
    //     return Name;
    // }
}
