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
        CompilerContext data = new(file);

        Result<IEnumerator<Token>> tokens = Lex(file, data);
        if (!tokens) return tokens.Error;

        CompilationUnit unit;
        try
        {
            Parser parser = new(data);
            unit = parser.Parse(tokens.Value);
            if (unit == null) return "Failed to parse " + file;
        }
        catch (Exception e)
        {
            return Options.Verbose > 0 ? e.ToString() : e.Message;
        }

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

        try
        {
            result = target.Emit(unit, data);
            if (!result.IsSuccess) return "Failed to emit qbe ir " + file;
        }
        catch (Exception e)
        {
            return Options.Verbose > 0 ? e.ToString() : e.Message;
        }

        return result;
    }

    private static Result<IEnumerator<Token>> Lex(string file, CompilerContext data)
    {
        try
        {
            Lexer lexer = new(data);
            IEnumerator<Token> test = lexer.Lex(File.OpenText(file), file);
            return new Result<IEnumerator<Token>>(test);
        }
        catch (Exception e)
        {
            return Options.Verbose > 0 ? e.ToString() : e.Message;
        }
    }

    private static CompilationUnit Parse(IEnumerator<Token> tokens, CompilerContext data)
    {
        Parser parser = new(data);
        return parser.Parse(tokens);
    }
}
