namespace BLang.Utility;

using System;
using System.Diagnostics;

public record Executable(string? StdOut, string? StdError)
{
    public static bool Run(string executable, string arguments = "")
    {
        Executable exe = Capture(executable, arguments);

        if (!string.IsNullOrEmpty(exe.StdError))
        {
            Error(exe.StdError, "ERR");
            return false;
        }

        Log(exe.StdOut, "OUT");
        return true;
    }

    public static Executable Capture(string executable, string arguments = "")
    {
        if (options.Debug) Info($"{executable} {arguments}", "CMD");

        using Process? process = Process.Start(new ProcessStartInfo(executable, arguments) { RedirectStandardOutput = true, RedirectStandardError = true });
        process?.WaitForExit();

        string? stdOut = process?.StandardOutput?.ReadToEnd();
        string? stdErr = process?.StandardError?.ReadToEnd();

        return new(stdOut, stdErr);
    }

    public bool HasError()
    {
        return !string.IsNullOrEmpty(StdError);
    }

    public bool HasOutput()
    {
        return !string.IsNullOrEmpty(StdOut);
    }
}
