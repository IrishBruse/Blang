namespace BLang.Ast;

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

    public Token PeekToken()
    {
        return tokens.Current;
    }

    private Token Next()
    {
        Token token = tokens.Current;
        if (token != null)
        {
            TokenPosition = token.Range;
        }

        if (Options.Tokens && token != null)
        {
            LogToken(token);
        }

        _ = tokens.MoveNext();

        return token!;
    }

    private static void LogToken(Token token)
    {
        string type = token.TokenType.ToString().Trim();
        string content = token.Content.Trim();

        if (token.TokenType == TokenType.IntegerLiteral)
        {
            content = Colors.Yellow(content);
        }
        else if (token.TokenType == TokenType.StringLiteral)
        {
            content = Colors.Green('"' + content + '"');
        }
        else if (token.TokenType.IsKeyword())
        {
            content = Colors.Magenta(content);
        }
        else if (token.TokenType == TokenType.Identifier)
        {
            content = Colors.White(content);
        }
        else if (token.TokenType == TokenType.OpenBracket || token.TokenType == TokenType.CloseBracket)
        {
            content = Colors.Cyan(content);
        }
        else if (token.TokenType == TokenType.OpenParenthesis || token.TokenType == TokenType.CloseParenthesis)
        {
            content = Colors.Cyan(content);
        }
        else if (token.TokenType == TokenType.OpenScope || token.TokenType == TokenType.CloseScope)
        {
            content = Colors.Cyan(content);
        }
        else if (token.TokenType == TokenType.Garbage)
        {
            content = Colors.Red(content);
        }
        else
        {
            content = Colors.Gray(content);
        }

        Console.WriteLine($"{type,-20} {content,-20}");
    }

    [StackTraceHidden]
    private Token Eat(TokenType type)
    {
        Token token = Next();

        if (token.TokenType != type)
        {
            throw new ParserException($"Expected {type} but got {token.TokenType}");
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

    private bool TryEat(TokenType type)
    {
        TokenType peek = Peek();

        if (peek != type)
        {
            return false;
        }

        _ = Next();
        return true;
    }

    private void EatComments()
    {
        while (Peek(TokenType.Comment))
        {
            _ = Eat(TokenType.Comment);
        }
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
