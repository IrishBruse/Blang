namespace IBlang;

using System.Diagnostics;
using System.Runtime.CompilerServices;

public class Context
{
    private const string LexerLogExt = ".lexer.log";
    private const string ParserLogExt = ".parser.log";

    public string CurrentFile { get; init; }
    public List<Span> LineSpans { get; set; }

    public Context(string file, List<Span> lineSpans)
    {
        CurrentFile = file;
        LineSpans = lineSpans;

        File.Delete(Path.ChangeExtension(CurrentFile, LexerLogExt));
        File.Delete(Path.ChangeExtension(CurrentFile, ParserLogExt));
    }

    [Conditional("DEBUG")]
    public void LogToken(string token)
    {
        File.AppendAllText(Path.ChangeExtension(CurrentFile, LexerLogExt), token + '\n');
    }

    [Conditional("DEBUG")]
    public void TraceParser([CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string method = "")
    {
        File.AppendAllText(Path.ChangeExtension(CurrentFile, ParserLogExt), $"{file}:{lineNumber} -> {method}()" + '\n');
    }
}
