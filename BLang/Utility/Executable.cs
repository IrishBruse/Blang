namespace BLang.Utility;

using System.Diagnostics;

public record Executable(string? StdOut, string? StdError, int ExitCode)
{
    public static bool Run(string executable, string arguments = "")
    {
        Executable exe = Capture(executable, arguments);

        if (!string.IsNullOrEmpty(exe.StdError))
        {
            Error(exe.StdError, "ERR");
            return false;
        }

        Log(exe.StdOut);
        return true;
    }

    public static Executable Capture(string executable, string arguments = "")
    {
        if (Options.Debug) Info($"{executable} {arguments}", "CMD");

        try
        {
            using Process? process = Process.Start(new ProcessStartInfo(executable, arguments) { RedirectStandardOutput = true, RedirectStandardError = true });
            process?.WaitForExit();

            string? stdOut = process?.StandardOutput?.ReadToEnd();
            string? stdErr = process?.StandardError?.ReadToEnd();
            return new(stdOut, stdErr, process?.ExitCode ?? -1);
        }
        catch (System.Exception e)
        {
            return new("", e.ToString(), 1);
        }
    }

    public bool HasError()
    {
        return !string.IsNullOrEmpty(StdError);
    }

    public bool HasOutput()
    {
        return !string.IsNullOrEmpty(StdOut);
    }

    public bool Success()
    {
        return ExitCode == 0;
    }
}
