namespace BLang.AstParser;

using System;
using System.Collections.Generic;
using BLang.Exceptions;
using BLang.Tokenizer;

public partial class Parser
{
    static readonly Dictionary<TokenType, int> operatorPrecedence = new(){
        { TokenType.BitwiseOr, 0 },
        { TokenType.BitwiseAnd, 1 },
        { TokenType.BitwiseShiftLeft, 2 }, { TokenType.BitwiseShiftRight, 2 },
        { TokenType.EqualEqual, 2 }, { TokenType.NotEqual, 2 },
        { TokenType.LessThan, 3 }, { TokenType.GreaterThan, 3 }, { TokenType.GreaterThanEqual, 3 }, { TokenType.LessThanEqual, 3 },
        { TokenType.Addition, 4 }, { TokenType.Subtraction, 4 },
        { TokenType.Multiplication, 5 }, { TokenType.Modulo, 5 }, { TokenType.Division, 5 },
    };

    BinaryExpression ParseBinaryExpression(int minPrecedence = 0)
    {
        BinaryExpression left = ParsePrimary();

        while (!Peek(TokenType.Eof) && operatorPrecedence.GetValueOrDefault(Peek(), -1) >= minPrecedence)
        {
            if (!operatorPrecedence.TryGetValue(Peek(), out int precedence))
            {
                throw new ParserException($"{data.GetFileLocation(previousTokenRange.End)} Unexpected token in {nameof(ParseBinaryExpression)} of type {Peek()}");
            }

            Token conditionalOperator = Next();

            Expression right = ParseBinaryExpression(precedence);

            left = new BinaryExpression(conditionalOperator.TokenType, left, right)
            {
                Range = left.Range.Merge(right.Range)
            };
        }

        return left;
    }

    BinaryExpression ParsePrimary()
    {
        return Peek() switch
        {
            TokenType.OpenParenthesis => ParseGroupExpression(),
            TokenType.IntegerLiteral => new BinaryExpression(TokenType.None, ParseInteger(), null),
            TokenType.Identifier => new BinaryExpression(TokenType.None, ParseVariable(), null),
            _ => throw new ParserException($"{data.GetFileLocation(previousTokenRange.End)} Unexpected token in {nameof(ParsePrimary)} of type {Peek()}"),
        };
    }

    BinaryExpression ParseGroupExpression()
    {
        Eat(TokenType.OpenParenthesis);
        BinaryExpression expr = ParseBinaryExpression();
        Eat(TokenType.CloseParenthesis);
        return expr;
    }
}
