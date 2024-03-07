namespace IBlang.Data;

using System.Diagnostics;

public record ParseError(string Message, Span? Span, StackTrace StackTrace)
{
}
