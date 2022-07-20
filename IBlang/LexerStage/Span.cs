namespace IBlang.LexerStage;

public record struct Loc(int Start, int End, int Line, int Column);
