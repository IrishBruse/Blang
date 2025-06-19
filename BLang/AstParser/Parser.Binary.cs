namespace BLang.AstParser;

using System;
using System.Collections.Generic;
using BLang.Exceptions;
using BLang.Tokenizer;

public partial class Parser
{
    static readonly Dictionary<TokenType, int> operatorPrecedence = new(){
        { TokenType.BitwiseOr, 5 },
        { TokenType.BitwiseAnd, 4 },
        { TokenType.BitwiseShiftLeft, 3 }, { TokenType.BitwiseShiftRight, 3 }, { TokenType.EqualEqual, 3 }, { TokenType.NotEqual, 3 },
        { TokenType.LessThan, 2 }, { TokenType.GreaterThan, 2 }, { TokenType.GreaterThanEqual, 2 }, { TokenType.LessThanEqual, 2 },
        { TokenType.Multiplication, 1 }, { TokenType.Modulo, 1 }, { TokenType.Division, 1 },
        { TokenType.Addition, 0 }, { TokenType.Subtraction, 0 },
    };

    Expression ParseBinaryExpression(int minPrecedence = 0)
    {
        Expression left = ParsePrimary();

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

    Expression ParsePrimary()
    {
        return Peek() switch
        {
            TokenType.OpenParenthesis => ParseGroupExpression(),
            TokenType.IntegerLiteral => ParseInteger(),
            TokenType.Identifier => ParseVariable(),
            _ => throw new ParserException($"{data.GetFileLocation(previousTokenRange.End)} Unexpected token in {nameof(ParsePrimary)} of type {Peek()}"),
        };
    }

    Expression ParseGroupExpression()
    {
        Eat(TokenType.OpenParenthesis);
        Expression expr = ParseBinaryExpression();
        Eat(TokenType.CloseParenthesis);
        return expr;
    }
}
