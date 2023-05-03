namespace IBlang;

public record Token(string Value, TokenType Type, int Start, int End) { }
