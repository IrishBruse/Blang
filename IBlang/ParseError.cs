namespace IBlang;

using System.Diagnostics;

public record ParseError(string Message, Span Span, StackTrace StackTrace)
{
}
