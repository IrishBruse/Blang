namespace BLang.Targets.qbe;

using System.Collections.Generic;
using System.Text;
using BLang.Utility;

public partial class QbeOutput()
{
    public virtual int Indention { get; } = 4;
    private readonly Dictionary<Symbol, int> ssaVersionCounters = [];

    public StringBuilder Text { get; set; } = new();
    private int Depth { get; set; }

    public void Indent(int? spaces = null)
    {
        Depth += spaces ?? Indention;
    }

    public void Unindent(int? spaces = null)
    {
        Depth -= spaces ?? Indention;
    }

    public string Space => new(' ', Depth);

    public void WriteRaw(string? value)
    {
        _ = Text.Append(value);
    }

    public void Write(string value)
    {
        _ = Text.AppendLine(Space + value);
        // throw new NotImplementedException();
    }

    public void WriteGen(string value)
    {
        _ = Text.AppendLine(Space + value);
    }

    public void BeginScope()
    {
        _ = Text.AppendLine(Space + "{");
        Indent();
    }

    public void EndScope()
    {
        Unindent();
        _ = Text.AppendLine(Space + "}");
    }

    public void WriteLine()
    {
        _ = Text.AppendLine();
    }

    public void Comment(params string[] message)
    {
        WriteLine();
        Write("# " + string.Join(' ', message));
    }

    public void Clear()
    {
        _ = Text.Clear();
        Depth = 0;
    }

    // QBE

    public void Label(string value)
    {
        Write("@" + value);
    }

    public void Function(string name, string returnType = "w", params string[] args)
    {
        WriteGen($"function {returnType} ${name}()");
    }

    public void ExportFunction(string name, string returnType = "w", params string[] args)
    {
        WriteGen($"export function {returnType} ${name}()");
    }

    public void Data(string name, string value)
    {
        WriteGen($"data ${name} = {{ b \"{value}\", b 0 }}");
    }

    private int tempReg;
    public string GetTempReg()
    {
        return "%temp_" + tempReg++;
    }

    public string CreateTempRegister(Symbol symbol)
    {
        ssaVersionCounters[symbol] = !ssaVersionCounters.TryGetValue(symbol, out int value) ? 0 : ++value;

        return $"%{symbol.Name}_{ssaVersionCounters[symbol]}";
    }

    /// <summary> Stores a word (32-bit) integer value into memory. </summary>
    public void Storew(int value, string address)
    {
        Storew(value.ToString(), address);
    }
}
