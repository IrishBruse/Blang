namespace IBlang;

using IBlang.Data;
using IBlang.Walker;

public static class Compiler
{
    public static string Run(string file, CompilationFlags flags)
    {
        file = Path.GetFullPath(file);

        Console.WriteLine($"Compiling {file}");

        StreamReader sourceFile = File.OpenText(file);

        Lexer lexer = new(sourceFile, file, flags);
        IEnumerator<Token> tokens = lexer.Lex();
        Project project = new(tokens, lexer.LineEndings);

        if (project.Errors.Count > 0)
        {
            PrintErrors(project);
            return string.Empty;
        }

        Parser parser = new(project);
        FileAst ast = parser.Parse();

        if (flags.HasFlag(CompilationFlags.Print))
        {
            AstVisitor debugVisitor = new(new PrintAstDebugger(), flags);
            debugVisitor.Visit(ast);
        }

        if (project.Errors.Count > 0)
        {
            PrintErrors(project);
            return string.Empty;
        }

        Transpiler transpiler = new(project);
        transpiler.TranspileToC(ast);

        string output = transpiler.Compile(ast, flags);

        if (project.Errors.Count > 0)
        {
            PrintErrors(project);
            return string.Empty;
        }

        return output;
    }

    public static bool Test(string file)
    {
        string[] lines = File.ReadAllLines(file);

        string expected = "";

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            if (line.StartsWith("//"))
            {
                expected += line[2..] + "\n";
            }
            else
            {
                break;
            }
        }

        string output = Run(file, CompilationFlags.Run);
        return expected.Equals(output, StringComparison.Ordinal);
    }

    public static void PrintErrors(Project tokens, bool showStackTrace = false)
    {
        Console.WriteLine("-------- Errors --------");

        Console.ForegroundColor = ConsoleColor.Red;
        foreach (ParseError error in tokens.Errors)
        {
            int line = 0;
            int column = 0;
            int lastIndex = 0;

            if (error.Span != null)
            {
                foreach ((int index, int newLine) in tokens.LineEndings)
                {
                    if (error.Span.Start >= lastIndex && error.Span.Start <= index)
                    {
                        line = newLine;
                        column = error.Span.Start - lastIndex;
                        break;
                    }

                    lastIndex = index;
                }

                Console.Error.WriteLine($"{error.Span.File}:{line}:{column} {error.Message}");
            }
            else
            {
                Console.Error.WriteLine(error.Message);
            }

            if (showStackTrace)
            {
                Console.Error.Write(error.StackTrace);
            }
        }
        Console.ResetColor();
    }
}
