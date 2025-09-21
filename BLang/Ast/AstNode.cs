namespace BLang.Ast;

using BLang.Utility;

public abstract record AstNode
{
#pragma warning disable CA1051
    public SourceRange Range = SourceRange.Zero;
#pragma warning restore CA1051
}
