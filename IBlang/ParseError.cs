namespace IBlang;

public record ParseError(string Message, Span Span)
{
}
