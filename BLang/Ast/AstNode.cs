namespace BLang.Ast;

using BLang.Utility;

#pragma warning disable CA1051

public abstract record AstNode
{
    public SourceRange Range = SourceRange.Zero;
}
