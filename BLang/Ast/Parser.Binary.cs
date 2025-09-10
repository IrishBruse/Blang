namespace BLang.Ast;

using System.Collections.Generic;

using BLang.Ast.Nodes;
using BLang.Exceptions;
using BLang.Tokenizer;

public partial class Parser
{
    private static readonly Dictionary<TokenType, int> operatorPrecedence = new(){
        { TokenType.BitwiseOr, 10 },
        { TokenType.BitwiseAnd, 20 },
        { TokenType.EqualEqual, 30 }, { TokenType.NotEqual, 30 },
        { TokenType.LessThan, 40 }, { TokenType.GreaterThan, 40 },
        { TokenType.GreaterThanEqual, 40 }, { TokenType.LessThanEqual, 40 },
        { TokenType.BitwiseShiftLeft, 50 }, { TokenType.BitwiseShiftRight, 50 },
        { TokenType.Addition, 60 }, { TokenType.Subtraction, 60 },
        { TokenType.Multiplication, 70 }, { TokenType.Modulo, 70 }, { TokenType.Division, 70 },
    };

    private Expression ParseBinaryExpression(int minPrecedence = 0)
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

    private Expression ParsePrimary()
    {
        return Peek() switch
        {
            TokenType.OpenParenthesis => ParseGroupExpression(),
            TokenType.IntegerLiteral => ParseInteger(),
            TokenType.Identifier => ParseVariable(),

            // Pointers
            TokenType.AddressOf => ParseAddressOf(),
            TokenType.PointerDereference => ParsePointerDereference(),
            TokenType.Eof => throw new System.NotImplementedException(),
            TokenType.Garbage => throw new System.NotImplementedException(),
            TokenType.None => throw new System.NotImplementedException(),
            TokenType.Comment => throw new System.NotImplementedException(),
            TokenType.FloatLiteral => throw new System.NotImplementedException(),
            TokenType.StringLiteral => throw new System.NotImplementedException(),
            TokenType.CharLiteral => throw new System.NotImplementedException(),
            TokenType.CloseParenthesis => throw new System.NotImplementedException(),
            TokenType.OpenBracket => throw new System.NotImplementedException(),
            TokenType.CloseBracket => throw new System.NotImplementedException(),
            TokenType.OpenScope => throw new System.NotImplementedException(),
            TokenType.CloseScope => throw new System.NotImplementedException(),
            TokenType.Dot => throw new System.NotImplementedException(),
            TokenType.Comma => throw new System.NotImplementedException(),
            TokenType.Addition => throw new System.NotImplementedException(),
            TokenType.Subtraction => throw new System.NotImplementedException(),
            TokenType.Division => throw new System.NotImplementedException(),
            TokenType.Modulo => throw new System.NotImplementedException(),
            TokenType.LessThan => throw new System.NotImplementedException(),
            TokenType.GreaterThan => throw new System.NotImplementedException(),
            TokenType.LessThanEqual => throw new System.NotImplementedException(),
            TokenType.GreaterThanEqual => throw new System.NotImplementedException(),
            TokenType.EqualEqual => throw new System.NotImplementedException(),
            TokenType.NotEqual => throw new System.NotImplementedException(),
            TokenType.LogicalAnd => throw new System.NotImplementedException(),
            TokenType.LogicalOr => throw new System.NotImplementedException(),
            TokenType.LogicalNot => throw new System.NotImplementedException(),
            TokenType.Assignment => throw new System.NotImplementedException(),
            TokenType.AdditionAssignment => throw new System.NotImplementedException(),
            TokenType.SubtractionAssignment => throw new System.NotImplementedException(),
            TokenType.MultiplicationAssignment => throw new System.NotImplementedException(),
            TokenType.DivisionAssignment => throw new System.NotImplementedException(),
            TokenType.ModuloAssignment => throw new System.NotImplementedException(),
            TokenType.Increment => throw new System.NotImplementedException(),
            TokenType.Decrement => throw new System.NotImplementedException(),
            TokenType.BitwiseComplement => throw new System.NotImplementedException(),
            TokenType.BitwiseOr => throw new System.NotImplementedException(),
            TokenType.BitwiseXOr => throw new System.NotImplementedException(),
            TokenType.BitwiseShiftLeft => throw new System.NotImplementedException(),
            TokenType.BitwiseShiftRight => throw new System.NotImplementedException(),
            TokenType.Semicolon => throw new System.NotImplementedException(),
            TokenType.ExternKeyword => throw new System.NotImplementedException(),
            TokenType.IfKeyword => throw new System.NotImplementedException(),
            TokenType.ElseKeyword => throw new System.NotImplementedException(),
            TokenType.WhileKeyword => throw new System.NotImplementedException(),
            TokenType.AutoKeyword => throw new System.NotImplementedException(),
            TokenType.SwitchKeyword => throw new System.NotImplementedException(),
            TokenType.CaseKeyword => throw new System.NotImplementedException(),
            TokenType.BreakKeyword => throw new System.NotImplementedException(),
            _ => throw new ParserException($"{data.GetFileLocation(previousTokenRange.End)} Unexpected token in {nameof(ParsePrimary)} of type {Peek()}"),
        };
    }

    private AddressOfExpression ParseAddressOf()
    {
        _ = Eat(TokenType.AddressOf);
        Expression expr = ParsePrimary();
        return new AddressOfExpression(expr) { Range = expr.Range };
    }

    private PointerDereferenceExpression ParsePointerDereference()
    {
        _ = Eat(TokenType.PointerDereference);
        Expression expr = ParsePrimary();
        return new PointerDereferenceExpression(expr) { Range = expr.Range };
    }

    private Expression ParseGroupExpression()
    {
        _ = Eat(TokenType.OpenParenthesis);
        Expression expr = ParseBinaryExpression();
        _ = Eat(TokenType.CloseParenthesis);
        return expr;
    }
}
