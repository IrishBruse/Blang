namespace BLang.Targets.qbe;

using System;
using System.Collections.Generic;
using System.Text;
using BLang.Exceptions;
using BLang.Utility;

public partial class QbeOutput()
{
    public virtual int Indention { get; } = 4;
    private readonly Dictionary<Symbol, int> ssaVersionCounters = [];

    public StringBuilder Text { get; set; } = new();
    private int Depth { get; set; }

    private readonly Dictionary<Symbol, string> memoryAddresses = [];

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

    public void Comment(string message)
    {
        if (Options.Verbose > 0)
        {
            Write("# " + message);
        }
    }

    public void DebugComment(string message)
    {
        if (Options.Memory)
        {
            Write("# " + message);
        }
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

    public void Label(string value)
    {
        Unindent();
        Write("@" + value);
        Indent();
    }

    public void Function(string name, string returnType = "l", params string[] args)
    {
        Write($"function {returnType} ${name}({string.Join(", ", args)})");
    }

    public void ExportFunction(string name, string returnType = "l", params string[] args)
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

    /// <summary> Stores a word (32-bit) integer value into memory. </summary>
    public void Storew(int value, string address)
    {
        Storew(value.ToString(), address);
    }

    /// <summary> Stores a long (64-bit) integer value into memory. </summary>
    public void Storel(int value, string address)
    {
        Storel(value.ToString(), address);
    }

    private Symbol? currentReg;
    public void SetRegisterName(Symbol sym)
    {
        currentReg = sym;
    }

    public string GetTempReg()
    {
        if (currentReg == null)
        {
            currentReg = new("temp");
        }
        return WriteRegister(currentReg);
    }

    public string WriteRegister(Symbol symbol)
    {
        SetRegisterName(symbol);

        int currentVersion = 0;
        // if (ssaVersionCounters.TryGetValue(symbol, out int value))
        // {
        //     currentVersion = value;
        // }

        if (Options.Memory)
        {
            Write($"# Reg(W): %{symbol.Name}_{currentVersion} -> %{symbol.Name}_{currentVersion + 1}");
        }

        return "write";
    }

    public string ReadRegister(Symbol symbol)
    {
        SetRegisterName(symbol);

        return "read";

        // int currentVersion = 0;
        // if (ssaVersionCounters.TryGetValue(symbol, out int value))
        // {
        //     currentVersion = value;
        // }

        // ssaVersionCounters[symbol] = currentVersion + 1;

        // if (Options.Memory)
        // {
        //     string comment = $"# {(increment ? "Write" : "Read")}: %{symbol.Name}_{currentVersion}";
        //     if (increment)
        //     {
        //         comment += $" -> %{symbol.Name}_{currentVersion + 1}";
        //     }
        //     Write(comment);
        // }

        // if (symbol.IsGlobal && currentVersion == -1)
        // {
        //     return $"${symbol.Name}";
        // }

        // return $"%{symbol.Name}_{currentVersion}";
    }

    public void CreateGlobalRegister(Symbol symbol)
    {
        if (!symbol.IsGlobal)
        {
            throw new ArgumentException(symbol.Name + " is not a global variable");
        }

        ssaVersionCounters[symbol] = -1;
    }

    public void ClearMemoryRegisters()
    {
        ssaVersionCounters.Clear();
        memoryAddresses.Clear();
    }
}

public enum Size
{
    W,
    L,
    S,
    D
}
