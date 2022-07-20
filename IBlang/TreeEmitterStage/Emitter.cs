namespace IBlang.TreeEmitterStage;

using IBlang.ParserStage;

public class Emitter
{
    private StreamWriter writer;

    public Emitter(string outputFile)
    {
        File.Delete(path: outputFile);
        writer = new(File.OpenWrite(outputFile));
    }

    public void Emit(Ast ast)
    {
        new Visitor(writer).Visit(ast);

        writer.Close();
    }
}
