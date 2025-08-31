namespace BLang.Utility;

public static class Colors
{
    public static string Reset<T>(T m) => $"\x1b[0m{m}\x1b[0m";
    public static string Bold<T>(T m) => $"\x1b[1m{m}\x1b[22m{m}\x1b[22m\x1b[1m";
    public static string Dim<T>(T m) => $"\x1b[2m{m}\x1b[22m{m}\x1b[22m\x1b[2m";
    public static string Italic<T>(T m) => $"\x1b[3m{m}\x1b[23m";
    public static string Underline<T>(T m) => $"\x1b[4m{m}\x1b[24m";
    public static string Inverse<T>(T m) => $"\x1b[7m{m}\x1b[27m";
    public static string Hidden<T>(T m) => $"\x1b[8m{m}\x1b[28m";
    public static string Strikethrough<T>(T m) => $"\x1b[9m{m}\x1b[29m";

    public static string Black<T>(T m) => $"\x1b[30m{m}\x1b[39m";
    public static string Red<T>(T m) => $"\x1b[31m{m}\x1b[39m";
    public static string Green<T>(T m) => $"\x1b[32m{m}\x1b[39m";
    public static string Yellow<T>(T m) => $"\x1b[33m{m}\x1b[39m";
    public static string Blue<T>(T m) => $"\x1b[34m{m}\x1b[39m";
    public static string Magenta<T>(T m) => $"\x1b[35m{m}\x1b[39m";
    public static string Cyan<T>(T m) => $"\x1b[36m{m}\x1b[39m";
    public static string White<T>(T m) => $"\x1b[37m{m}\x1b[39m";
    public static string Gray<T>(T m) => $"\x1b[90m{m}\x1b[39m";

    public static string BgBlack<T>(T m) => $"\x1b[40m{m}\x1b[49m";
    public static string BgRed<T>(T m) => $"\x1b[41m{m}\x1b[49m";
    public static string BgGreen<T>(T m) => $"\x1b[42m{m}\x1b[49m";
    public static string BgYellow<T>(T m) => $"\x1b[43m{m}\x1b[49m";
    public static string BgBlue<T>(T m) => $"\x1b[44m{m}\x1b[49m";
    public static string BgMagenta<T>(T m) => $"\x1b[45m{m}\x1b[49m";
    public static string BgCyan<T>(T m) => $"\x1b[46m{m}\x1b[49m";
    public static string BgWhite<T>(T m) => $"\x1b[47m{m}\x1b[49m";

    public static string BlackBright<T>(T m) => $"\x1b[90m{m}\x1b[39m";
    public static string RedBright<T>(T m) => $"\x1b[91m{m}\x1b[39m";
    public static string GreenBright<T>(T m) => $"\x1b[92m{m}\x1b[39m";
    public static string YellowBright<T>(T m) => $"\x1b[93m{m}\x1b[39m";
    public static string BlueBright<T>(T m) => $"\x1b[94m{m}\x1b[39m";
    public static string MagentaBright<T>(T m) => $"\x1b[95m{m}\x1b[39m";
    public static string CyanBright<T>(T m) => $"\x1b[96m{m}\x1b[39m";
    public static string WhiteBright<T>(T m) => $"\x1b[97m{m}\x1b[39m";

    public static string BgBlackBright<T>(T m) => $"\x1b[100m{m}\x1b[49m";
    public static string BgRedBright<T>(T m) => $"\x1b[101m{m}\x1b[49m";
    public static string BgGreenBright<T>(T m) => $"\x1b[102m{m}\x1b[49m";
    public static string BgYellowBright<T>(T m) => $"\x1b[103m{m}\x1b[49m";
    public static string BgBlueBright<T>(T m) => $"\x1b[104m{m}\x1b[49m";
    public static string BgMagentaBright<T>(T m) => $"\x1b[105m{m}\x1b[49m";
    public static string BgCyanBright<T>(T m) => $"\x1b[106m{m}\x1b[49m";
    public static string BgWhiteBright<T>(T m) => $"\x1b[107m{m}\x1b[49m";
}
