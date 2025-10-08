namespace BLang;

using System;
using System.Collections.Generic;
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
        CompilerContext data = new(file);

        Lexer lexer = new(data);
        IEnumerator<Token> tokens = lexer.Lex(File.OpenText(file), file);

        CompilationUnit unit;

        Parser parser = new(data);
        unit = parser.Parse(tokens);
        if (unit == null) return "Failed to parse " + file;

        if (Options.Ast)
        {
            string astFile = Path.ChangeExtension(file, "ast.json");
            File.WriteAllText(astFile, unit.ToJson());
        }

        QbeTarget target = Options.Target switch
        {
            CompilationTarget.Qbe => new QbeTarget(),
            _ => throw new CompilerException($"Unknown Target {Options.Target}"),
        };

        Result<CompileOutput> result;


        result = target.Emit(unit, data);
        if (!result.IsSuccess) return "Failed to emit qbe ir " + file;

        return result;
    }
}
