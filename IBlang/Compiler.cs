namespace IBlang;

using IBlang.Data;
using IBlang.Walker;

public class Compiler
{
    public static void Run(string file)
    {
        file = Path.GetFullPath(file);

        Console.WriteLine($"Compiling {file}");
        StreamReader sourceFile = File.OpenText(file);

        Project project = new();

        Console.WriteLine("-------- Lexer  --------");
        Lexer lexer = new(sourceFile, file, LexerDebug.Print);
        Tokens tokens = new(lexer.Lex(), lexer.LineEndings);

        if (tokens.Errors.Count > 0)
        {
            PrintErrors(tokens);
            return;
        }

        Console.WriteLine("-------- Parser --------");
        Parser parser = new(tokens);
        FileAst ast = parser.Parse();
        AstVisitor debugVisitor = new(new PrintAstDebugger());
        debugVisitor.Visit(ast);

        if (tokens.Errors.Count > 0)
        {
            PrintErrors(tokens);
            return;
        }

        Console.WriteLine("-------- TypeChecker --------");
        TypeChecker typeChecker = new(project);
        typeChecker.Check(ast);

        if (tokens.Errors.Count > 0)
        {
            PrintErrors(tokens);
            return;
        }

        Console.WriteLine("-------- Transpiler --------");
        Transpiler transpiler = new(tokens);
        transpiler.TranspileToC(ast);
        transpiler.Compile(ast);

        if (tokens.Errors.Count > 0)
        {
            PrintErrors(tokens);
            return;
        }

    }

    public static bool Test(string file)
    {
        string source = File.ReadAllText(file);

        string[] lines = source.Split("\n");

        string expected = "";

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            if (line.StartsWith("//"))
            {
                expected += line[2..] + "\n";
            }
        }

        Run(file);

        return expected.Equals(source, StringComparison.Ordinal);
    }

    public static void PrintErrors(Tokens tokens, bool showStackTrace = false)
    {
        if (tokens.Errors.Count > 0)
        {
            Console.Write("-------- ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Errors");
            Console.ResetColor();
            Console.WriteLine(" --------");
        }

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
