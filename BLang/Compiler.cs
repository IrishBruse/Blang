namespace BLang;

using System.Collections.Generic;
using System.Diagnostics;
using BLang.Ast;
using System.IO;
using BLang.Exceptions;
using BLang.Targets.qbe;
using BLang.Tokenizer;
using BLang.Utility;

public static class Compiler
{
    private static Stopwatch Timer = Stopwatch.StartNew();

    public static CompileOutput Compile(string file)
    {
        CompilerContext data = new(file);
        CompileOutput output = new(file);

        IEnumerator<Token> tokens = Lex(file, data);

        Result<CompilationUnit> parseResult = Parse(tokens, data);
        if (parseResult.IsFailure)
        {
            output.Error = parseResult.Error;
            return output;
        }

        Result<EmitOutput> emitResult = Emit(parseResult.Value, data);
        if (emitResult.IsFailure)
        {
            output.Error = emitResult.Error;
            return output;
        }
        EmitOutput emitOutput = emitResult.Value;

        // TODO: cleanup Emit above
        output.Executable = emitOutput.Executable;
        output.CompilationUnit = emitOutput.CompilationUnit;

        output.Success = true;
        return output;
    }

    private static IEnumerator<Token> Lex(string file, CompilerContext data)
    {
        Lexer lexer = new(data);
        IEnumerator<Token> tokens = lexer.Lex(File.OpenRead(file), file);
        return tokens;
    }

    private static Result<CompilationUnit> Parse(IEnumerator<Token> tokens, CompilerContext data)
    {
        if (Options.Verbose >= 2) Timer.Restart();

        Parser parser = new(data);
        Result<CompilationUnit> result = parser.Parse(tokens);
        if (result.IsFailure) return result.Error;

        if (Options.Ast)
        {
            string astFile = Path.ChangeExtension(data.File, "ast.json");
            File.WriteAllText(astFile, result.Value.ToJson());
        }

        if (Options.Verbose >= 2) Debug($"Parse Complete ({Timer.Elapsed.TotalMilliseconds}ms)");

        return result;
    }

    private static Result<EmitOutput> Emit(CompilationUnit unit, CompilerContext data)
    {
        if (Options.Verbose >= 2) Timer.Restart();

        QbeTarget target = Options.Target switch
        {
            CompilationTarget.Qbe => new QbeTarget(),
            _ => throw new CompilerException($"Unknown Target {Options.Target}"),
        };

        Result<EmitOutput> result = target.Emit(unit, data);
        if (result.IsFailure) return $"{data.File} Emit Error:\n{result.Error}";

        if (Options.Verbose >= 2) Debug($"Emitting Complete ({Timer.Elapsed.TotalMilliseconds}ms)");

        return result;
    }
}
