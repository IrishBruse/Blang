namespace IBlang.ParserStage;

using System;
using System.Collections.Generic;

using IBlang.LexerStage;

public partial class Parser
{
    private Context ctx;

    public Parser(Context ctx, Token[] tokens)
    {
        this.ctx = ctx;
        this.tokens = tokens;
        PeekToken = tokens[0];
        currentTokenIndex = 0;
    }

    public Ast Parse()
    {
        List<FunctionDecleration> functions = new();
        while (PeekToken.Type != TokenType.Eof)
        {
            switch (PeekToken.Type)
            {
                case TokenType.Keyword_Func: functions.Add(ParseFuncDecleration()); break;

                default:
                Log.Error($"{ctx.Files[0]} Unhandled token of type {PeekToken.Type}");
                NextToken();
                break;
            }
        }

        return new Ast(functions.ToArray());
    }

    private FunctionDecleration ParseFuncDecleration()
    {
        EatToken(TokenType.Keyword_Func);
        var identifier = EatIdentifier();
        EatToken(TokenType.OpenParenthesis);

        List<Identifier> parameters = new();
        while (PeekToken.Type != TokenType.CloseParenthesis)
        {
            Token token = NextToken();
            parameters.Add(new(token.Value));

            if (PeekToken.Type == TokenType.Comma)
            {
                Log.Error($" TODO: add comma handling");
            }
        }

        EatToken(TokenType.CloseParenthesis);

        EatOptionalToken(TokenType.Eol);

        EatToken(TokenType.OpenScope);

        List<Node> exprs = new();
        while (PeekToken.Type != TokenType.CloseScope)
        {
            Node expr = ParseExpressions();
            if (expr is GarbageExpression garbageExpression)
            {
                Log.Error($" GarbageExpression Encountered! {garbageExpression.ErrorToken}");
                return new FunctionDecleration(identifier, parameters.ToArray(), exprs.ToArray());
            }
            exprs.Add(expr);
        }

        EatToken(TokenType.CloseScope);

        return new FunctionDecleration(identifier, parameters.ToArray(), exprs.ToArray());
    }

    private Node ParseExpressions()
    {
        return PeekToken.Type switch
        {
            TokenType.Identifier => ParseVarOrCall(),
            TokenType.NumberLiteral => new ValueLiteral(ValueType.Int, NextToken().Value),
            _ => new GarbageExpression(PeekToken),
        };
    }

    private Node ParseVarOrCall()
    {
        string identifier = EatIdentifier();

        return PeekToken.Type switch
        {
            TokenType.OpenParenthesis => ParseFuncCall(identifier),
            TokenType.Equal => ParseVariableDecleration(identifier),
            _ => new GarbageExpression(PeekToken)
        };
    }

    private Node ParseFuncCall(string identifier)
    {
        EatToken(TokenType.OpenParenthesis);

        List<ValueLiteral> args = new();
        while (PeekToken.Type != TokenType.CloseParenthesis)
        {
            Token token = EatToken(TokenType.StringLiteral);
            args.Add(new(ValueType.String, token.Value));

            EatOptionalToken(TokenType.Comma);
        }

        EatToken(TokenType.CloseParenthesis);
        return new FunctionCallExpression(identifier, args.ToArray());
    }

    private Node ParseVariableDecleration(string identifier)
    {
        EatToken(TokenType.Equal);

        return new BinaryExpression(new Identifier(identifier), ParseExpressions());
    }
}
