namespace IBlang;

public record Context(string[] Files, List<Span> LineSpans) { }

public record struct Span(int Start, int End);
