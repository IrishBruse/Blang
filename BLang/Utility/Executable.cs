namespace BLang.Utility;

using System;
using System.Diagnostics;

public class Executable
{
    public static string? Run(string executable, string arguments = "")
    {
        Console.WriteLine($"[CMD] {executable} {arguments}");
        using Process? process = Process.Start(new ProcessStartInfo(executable, arguments) { RedirectStandardOutput = true, RedirectStandardError = true });
        process?.WaitForExit();

        string? output = process?.StandardOutput?.ReadToEnd();
        string? errorOutput = process?.StandardError?.ReadToEnd();

        Terminal.Error(errorOutput);

        return output;
    }
}
