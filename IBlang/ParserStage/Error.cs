namespace IBlang.ParserStage;

public record struct Error(string Mesage)
{
    public static readonly Error None = new("");
}
