namespace BLang.Utility;

public record Symbol(string Name)
{
    public override string ToString()
    {
        return Name;
    }
}
