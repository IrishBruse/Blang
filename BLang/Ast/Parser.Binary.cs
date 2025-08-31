namespace BLang.Ast;

using System.Collections.Generic;

using BLang.Ast.Nodes;
using BLang.Exceptions;
using BLang.Tokenizer;

public partial class Parser
{
    static readonly Dictionary<TokenType, int> operatorPrecedence = new(){
        { TokenType.BitwiseOr, 10 },
        { TokenType.BitwiseAnd, 20 },
        { TokenType.EqualEqual, 30 }, { TokenType.NotEqual, 30 },
        { TokenType.LessThan, 40 }, { TokenType.GreaterThan, 40 },
        { TokenType.GreaterThanEqual, 40 }, { TokenType.LessThanEqual, 40 },
        { TokenType.BitwiseShiftLeft, 50 }, { TokenType.BitwiseShiftRight, 50 },
        { TokenType.Addition, 60 }, { TokenType.Subtraction, 60 },
        { TokenType.Multiplication, 70 }, { TokenType.Modulo, 70 }, { TokenType.Division, 70 },
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

            Expression right = ParseBinaryExpression(precedence + 1);

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

            // Pointers
            TokenType.AddressOf => ParseAddressOf(),
            TokenType.PointerDereference => ParsePointerDereference(),

            _ => throw new ParserException($"{data.GetFileLocation(previousTokenRange.End)} Unexpected token in {nameof(ParsePrimary)} of type {Peek()}"),
        };
    }

    AddressOfExpression ParseAddressOf()
    {
        Eat(TokenType.AddressOf);
        Expression expr = ParsePrimary();
        return new AddressOfExpression(expr) { Range = expr.Range };
    }

    PointerDereferenceExpression ParsePointerDereference()
    {
        Eat(TokenType.PointerDereference);
        Expression expr = ParsePrimary();
        return new PointerDereferenceExpression(expr) { Range = expr.Range };
    }

    Expression ParseGroupExpression()
    {
        Eat(TokenType.OpenParenthesis);
        Expression expr = ParseBinaryExpression();
        Eat(TokenType.CloseParenthesis);
        return expr;
    }
}
