namespace BLang.Utility;

using System;
using System.Diagnostics;
using System.Text;

public record Executable(string? StdOut, string? StdError, int ExitCode)
{
    /// <summary>
    /// Returns true when ExitCode is 0
    /// </summary>
    public bool Success
    {
        get
        {
            return ExitCode == 0;
        }
    }

    public static Executable Run(string executable, string arguments = "")
    {
        if (Options.Verbose > 1) Info($"{executable} {arguments}", "CMD");

        try
        {
            using Process? process = Process.Start(new ProcessStartInfo(executable, arguments) { RedirectStandardOutput = true, RedirectStandardError = true });
            process?.WaitForExit();

            string? stdOut = process?.StandardOutput?.ReadToEnd();
            string? stdErr = process?.StandardError?.ReadToEnd();
            return new(stdOut, stdErr, process?.ExitCode ?? -1);
        }
        catch (Exception e)
        {
            return new("", e.ToString(), 1);
        }
    }

    public Executable PipeErrorTo(StringBuilder target)
    {
        _ = target.Clear();
        if (StdError != null)
        {
            _ = target.Append(StdError.Trim());
        }

        return this;
    }

    public Executable PipeOutputTo(StringBuilder target)
    {
        if (StdOut != null)
        {
            _ = target.Append(StdOut);
        }

        return this;
    }

    public Executable PipeTo(StringBuilder target)
    {
        return PipeErrorTo(target).PipeOutputTo(target);
    }
}
