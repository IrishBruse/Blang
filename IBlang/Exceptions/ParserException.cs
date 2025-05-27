namespace IBlang.Exceptions;

using System;

public class ParserException : Exception
{
    public string Error { get; }

    public ParserException(string error) : base(error)
    {
        Error = error;
    }

    public ParserException(string error, Exception inner) : base(error, inner)
    {
        Error = error;
    }
}
