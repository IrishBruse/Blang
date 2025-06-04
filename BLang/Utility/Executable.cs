namespace BLang.Utility;

using System.Diagnostics;

public class Executable
{
    public static Result<string, Error<string>> Run(string executable, string arguments = "")
    {
        Debug($"{executable} {arguments}", "CMD");

        using Process? process = Process.Start(new ProcessStartInfo(executable, arguments) { RedirectStandardOutput = true, RedirectStandardError = true });
        process?.WaitForExit();

        string? output = process?.StandardOutput?.ReadToEnd();
        string? errorOutput = process?.StandardError?.ReadToEnd();

        if (!string.IsNullOrEmpty(errorOutput))
        {
            return Result<string, Error<string>>.Fail(new(errorOutput!));
        }

        return Result<string, Error<string>>.Ok(output);
    }
}
