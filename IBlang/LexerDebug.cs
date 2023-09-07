namespace IBlang;

using System;

[Flags]
public enum LexerDebug
{
    None,
    Print,
    Whitespace,
    Tokens
}
