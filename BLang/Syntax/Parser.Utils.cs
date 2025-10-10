namespace BLang.Ast;

using System;
using System.Diagnostics;
using BLang.Ast.Nodes;
using BLang.Exceptions;
using BLang.Tokenizer;
using BLang.Utility;

public partial class Parser
{
    public bool Peek(TokenType type)
    {
        return tokens.Current.TokenType == type;
    }

    public TokenType Peek()
    {
        return tokens.Current.TokenType;
    }

    private Token Next()
    {
        Token token = tokens.Current;
        if (token != null)
        {
            previousTokenRange = token.Range;
        }

        _ = tokens.MoveNext();
        if (Options.Tokens)
        {
            Console.WriteLine(tokens.Current);
        }

        return token!;
    }

    [StackTraceHidden]
    private Token Eat(TokenType type)
    {
        Token token = Next();

        if (token.TokenType != type)
        {
            int lastEnd = previousTokenRange.End;
            throw new ParserException(data.GetFileLocation(lastEnd) + $" Expected {type} but got {token.TokenType}");
        }

        return token;
    }

    private bool TryEat(TokenType type, out Token? token)
    {
        TokenType peek = Peek();

        if (peek != type)
        {
            token = null;
            return false;
        }

        token = Next();
        return true;
    }

    private void EatComments()
    {
        while (Peek(TokenType.Comment))
        {
            _ = Eat(TokenType.Comment);
        }
    }

    private Expression ParseIdentifier()
    {
        Token variable = Eat(TokenType.Identifier);
        Symbol? symbol = symbols.GetOrAdd(variable, SymbolKind.Load);
        if (symbol == null)
        {
            string loc = data.GetFileLocation(variable.Range.Start);
            throw new ParserException($"{loc}  {variable}");
        }

        // array[index] -> * (array + index)
        if (Peek(TokenType.OpenBracket))
        {
            _ = Eat(TokenType.OpenBracket);
            Token index = Eat(TokenType.IntegerLiteral);
            _ = Eat(TokenType.CloseBracket);

            // Create a pointer dereference expression
            return new ArrayIndexExpression(
                    new Variable(symbol) { Range = variable.Range }, index.Number
            )
            {
                Range = variable.Range
            };
        }

        return new Variable(symbol)
        {
            Range = variable.Range
        };
    }

    private IntValue ParseInteger(TokenType? prefix = null)
    {
        if (prefix != null)
        {
            _ = Next();
        }

        Token integer = Eat(TokenType.IntegerLiteral);

        string numberText = integer.Content;

        if (prefix == TokenType.Subtraction)
        {
            numberText = "-" + numberText;
        }

        if (!int.TryParse(numberText, out int number))
        {
            string loc = data.GetFileLocation(integer.Range.Start);
            throw new ParserException($"{loc} {integer} larger than 32bits");
        }

        return new IntValue(number)
        {
            Range = integer.Range
        };
    }

    private StringValue ParseString()
    {
        Token str = Eat(TokenType.StringLiteral);
        return new StringValue(str.Content)
        {
            Range = str.Range
        };
    }
}
