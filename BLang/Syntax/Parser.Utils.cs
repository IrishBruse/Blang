namespace BLang.Ast;

using System;

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
        token = Next();

        if (token.TokenType != type)
        {
            return false;
        }

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
            Expression index = ParseIdentifier();
            _ = Eat(TokenType.CloseBracket);

            // Create a pointer dereference expression
            return new BinaryExpression(
                    TokenType.ArrayIndexing,
                    new Variable(symbol) { Range = variable.Range },
                    index
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

    private IntValue ParseInteger()
    {
        Token integer = Eat(TokenType.IntegerLiteral);

        if (!int.TryParse(integer.Content, out int number))
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
