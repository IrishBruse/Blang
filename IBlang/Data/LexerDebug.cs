namespace IBlang.Data;

using System;

[Flags]
public enum CompilationFlags
{
    None,
    Print,
    Whitespace,
    Tokens,
    Run,
}
