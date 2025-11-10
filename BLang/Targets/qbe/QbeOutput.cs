namespace BLang.Targets.qbe;

using System;
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

    private void Write(string value)
    {
        _ = Text.AppendLine(Space + value);
        labelLast = false;
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
        if (!labelLast)
        {
            WriteLine();
        }
        Write("# " + string.Join(' ', message));
    }

    public void Clear()
    {
        _ = Text.Clear();
        Depth = 0;
    }

    public static char ToChar(Size type)
    {
        return type switch
        {
            Size.W => 'w',
            Size.L => 'l',
            Size.S => 's',
            Size.D => 'd',
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Invalid QBE type: {type}")
        };
    }

    // QBE

    private bool labelLast;
    public void Label(string value)
    {
        Unindent();
        Write("@" + value);
        Indent();
        labelLast = true;
    }

    public void Function(string name, string returnType = "w", params string[] args)
    {
        Write($"function {returnType} ${name}()");
    }

    public void ExportFunction(string name, string returnType = "w", params string[] args)
    {
        Write($"export function {returnType} ${name}()");
    }

    public void DataString(string name, string value)
    {
        Write($"data ${name} = {{ b \"{value}\", b 0 }}");
    }

    public void Data(string name, string data)
    {
        Write($"data ${name} = {{ {data} }}");
    }

    public void DataArray(string name, string value)
    {
        Write($"data ${name} = {{ {value}, b 0 }}");
    }

    private string? tempRegName;
    public void SetTempRegName(Symbol sym)
    {
        tempRegName = sym.Name;
    }
    public void SetTempRegName(string name)
    {
        tempRegName = name;
    }

    private int tempRegCounter;
    public string GetTempReg()
    {
        string name = tempRegName ?? "temp";
        return "%" + name + "_" + tempRegCounter++;
    }

    /// <summary> Stores a word (32-bit) integer value into memory. </summary>
    public void Storew(int value, string address)
    {
        Storew(value.ToString(), address);
    }
}

public enum Size
{
    W,
    L,
    S,
    D
}
