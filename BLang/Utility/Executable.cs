namespace BLang.Utility;

using System;
using System.Diagnostics;

public record Executable(string? StdOut, string? StdError, int ExitCode)
{
    public static Executable Capture(string executable, string arguments = "")
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

    public static bool Run(string executable, string arguments = "")
    {
        Executable exe = Capture(executable, arguments);
        if (!exe.Success())
        {
            return false;
        }
        return true;
    }

    public bool Success()
    {
        return ExitCode == 0;
    }
}
