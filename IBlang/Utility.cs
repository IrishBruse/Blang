namespace IBlang;

public record Context(string[] Files) { }

public record struct Span(int Start, int End);
