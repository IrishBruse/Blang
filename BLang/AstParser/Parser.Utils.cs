namespace BLang.AstParser;

using System;
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

    Token Next()
    {
        Token token = tokens.Current;
        previousTokenRange = token.Range;
        tokens.MoveNext();

        if (options.Tokens)
        {
            Console.WriteLine(tokens.Current);
        }

        return token;
    }

    Token Eat(TokenType type)
    {
        Token token = Next();

        if (token.TokenType != type)
        {
            int lastEnd = previousTokenRange.End;
            throw new ParserException(data.GetFileLocation(lastEnd) + $" Expected {type} but got {token.TokenType}");
        }

        return token;
    }

    void EatComments()
    {
        while (Peek(TokenType.Comment))
        {
            Eat(TokenType.Comment);
        }
    }

    Variable ParseVariable()
    {
        Token variable = Eat(TokenType.Identifier);
        Symbol? symbol = symbols.Get(variable.Content);
        if (symbol == null)
        {
            throw new Exception(variable.ToString());
        }
        return new Variable(symbol)
        {
            Range = variable.Range
        };
    }

    IntValue ParseInteger()
    {
        Token integer = Eat(TokenType.IntegerLiteral);
        return new IntValue(int.Parse(integer.Content))
        {
            Range = integer.Range
        };
    }

    StringValue ParseString()
    {
        Token str = Eat(TokenType.StringLiteral);
        return new StringValue(str.Content)
        {
            Range = str.Range
        };
    }
}
