namespace IBlang;

using System;
using System.IO;

public class TargetWriter(params TextWriter[] writers) : IDisposable
{
    public void Write(char value)
    {
        foreach (TextWriter writer in writers)
        {
            writer.Write(value);
        }
    }

    public void Write(char[] buffer, int index, int count)
    {
        foreach (TextWriter writer in writers)
        {
            writer.Write(buffer, index, count);
        }
    }

    public void Write(string? value)
    {
        foreach (TextWriter writer in writers)
        {
            writer.Write(value);
        }
    }

    public void WriteLine(string? value)
    {
        foreach (TextWriter writer in writers)
        {
            writer.WriteLine(value);
        }
    }

    public void WriteLine()
    {
        foreach (TextWriter writer in writers)
        {
            writer.WriteLine();
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        foreach (TextWriter writer in writers)
        {
            writer.Flush();
        }

        foreach (TextWriter writer in writers)
        {
            writer.Dispose();
        }
    }
}
