namespace BLang.Exceptions;

using System;

public class ParserException(string error) : Exception(error)
{
    public string Error { get; } = error;
}
