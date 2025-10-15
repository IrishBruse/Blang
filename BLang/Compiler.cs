namespace BLang;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using BLang.Ast;
using BLang.Exceptions;
using BLang.Targets.qbe;
using BLang.Tokenizer;
using BLang.Utility;

public static class Compiler
{
    public static Result<CompileOutput> Compile(string file)
    {
        try
        {
            return CompileFile(file);
        }
        catch (Exception e)
        {
            return Options.Verbose > 0 ? e.ToString() : e.Message;
        }
    }

    private static Result<CompileOutput> CompileFile(string file)
    {
        Stopwatch sw = Stopwatch.StartNew();

        CompilerContext data = new(file);

        IEnumerator<Token> tokens = Lex(file, data);

        Result<CompilationUnit> parseResult = Parse(tokens, data);
        if (parseResult.IsFailure) return parseResult.Error;

        long elapsedTime = sw.ElapsedMilliseconds;

        Result<CompileOutput> emitResult = Emit(parseResult.Value, data);
        if (emitResult.IsFailure) return emitResult.Error;

        emitResult.Value.CompileTime = elapsedTime;
        return emitResult.Value;
    }

    private static IEnumerator<Token> Lex(string file, CompilerContext data)
    {
        Lexer lexer = new(data);
        IEnumerator<Token> tokens = lexer.Lex(File.OpenRead(file), file);
        return tokens;
    }

    private static Result<CompilationUnit> Parse(IEnumerator<Token> tokens, CompilerContext data)
    {
        Parser parser = new(data);
        Result<CompilationUnit> parseResult = parser.Parse(tokens);
        if (parseResult.IsFailure) return parseResult.Error;

        if (Options.Ast)
        {
            string astFile = Path.ChangeExtension(data.File, "ast.json");
            File.WriteAllText(astFile, parseResult.Value.ToJson());
        }

        return parseResult;
    }

    private static Result<CompileOutput> Emit(CompilationUnit unit, CompilerContext data)
    {
        QbeTarget target = Options.Target switch
        {
            CompilationTarget.Qbe => new QbeTarget(),
            _ => throw new CompilerException($"Unknown Target {Options.Target}"),
        };

        Result<CompileOutput> result = target.Emit(unit, data);
        if (result.IsFailure) return data.File + " Failed to emit " + result.Error;

        return result;
    }
}
