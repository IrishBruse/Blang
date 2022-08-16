namespace IBlang.Stage4Lowering;

using IBlang.Stage2Parser;

public class AstLowering
{
    private readonly Context ctx;

    public AstLowering(Context ctx)
    {
        this.ctx = ctx;
    }

    public Ast Lower(Ast ast)
    {
        return ast;
    }
}
