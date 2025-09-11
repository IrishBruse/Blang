namespace BLang.Ast;

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BLang.Ast.Nodes;
using BLang.Exceptions;
using BLang.Tokenizer;
using BLang.Utility;

public partial class Parser
{
    private static readonly Dictionary<TokenType, int> OperatorPrecedence = new(){
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

        while (!Peek(TokenType.Eof) && OperatorPrecedence.GetValueOrDefault(Peek(), -1) >= minPrecedence)
        {
            if (!OperatorPrecedence.TryGetValue(Peek(), out int precedence))
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
            TokenType.Identifier => ParseIdentifier(),

            // Pointers
            TokenType.AddressOf => ParseAddressOf(),
            TokenType.PointerDereference => ParsePointerDereference(),

            //
            TokenType.Eof => Unexpected(data),
            TokenType.Garbage => Unexpected(data),
            TokenType.None => Unexpected(data),
            TokenType.Comment => Unexpected(data),
            TokenType.FloatLiteral => Unexpected(data),
            TokenType.StringLiteral => Unexpected(data),
            TokenType.CharLiteral => Unexpected(data),
            TokenType.CloseParenthesis => Unexpected(data),
            TokenType.OpenBracket => Unexpected(data),
            TokenType.CloseBracket => Unexpected(data),
            TokenType.OpenScope => Unexpected(data),
            TokenType.CloseScope => Unexpected(data),
            TokenType.Dot => Unexpected(data),
            TokenType.Comma => Unexpected(data),
            TokenType.Addition => Unexpected(data),
            TokenType.Subtraction => Unexpected(data),
            TokenType.Division => Unexpected(data),
            TokenType.Modulo => Unexpected(data),
            TokenType.LessThan => Unexpected(data),
            TokenType.GreaterThan => Unexpected(data),
            TokenType.LessThanEqual => Unexpected(data),
            TokenType.GreaterThanEqual => Unexpected(data),
            TokenType.EqualEqual => Unexpected(data),
            TokenType.NotEqual => Unexpected(data),
            TokenType.LogicalAnd => Unexpected(data),
            TokenType.LogicalOr => Unexpected(data),
            TokenType.LogicalNot => Unexpected(data),
            TokenType.Assignment => Unexpected(data),
            TokenType.AdditionAssignment => Unexpected(data),
            TokenType.SubtractionAssignment => Unexpected(data),
            TokenType.MultiplicationAssignment => Unexpected(data),
            TokenType.DivisionAssignment => Unexpected(data),
            TokenType.ModuloAssignment => Unexpected(data),
            TokenType.Increment => Unexpected(data),
            TokenType.Decrement => Unexpected(data),
            TokenType.BitwiseComplement => Unexpected(data),
            TokenType.BitwiseOr => Unexpected(data),
            TokenType.BitwiseXOr => Unexpected(data),
            TokenType.BitwiseShiftLeft => Unexpected(data),
            TokenType.BitwiseShiftRight => Unexpected(data),
            TokenType.Semicolon => Unexpected(data),
            TokenType.ExternKeyword => Unexpected(data),
            TokenType.IfKeyword => Unexpected(data),
            TokenType.ElseKeyword => Unexpected(data),
            TokenType.WhileKeyword => Unexpected(data),
            TokenType.AutoKeyword => Unexpected(data),
            TokenType.SwitchKeyword => Unexpected(data),
            TokenType.CaseKeyword => Unexpected(data),
            TokenType.BreakKeyword => Unexpected(data),
            _ => Unexpected(data),
        };
    }

    private Expression Unexpected(CompilationData data, [CallerMemberName] string callerName = "")
    {
        throw new ParserException($"{data.GetFileLocation(previousTokenRange.End)} Unexpected token in {callerName} of type {Peek()}");
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
