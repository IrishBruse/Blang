namespace BLang.Exceptions;

using System;

public class CompilerException(string error) : Exception(error)
{
    public string Error { get; } = error;
}
