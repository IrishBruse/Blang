namespace IBlang.Stage2Parser;

using System;
using System.Collections.Generic;

using IBlang.Stage1Lexer;

public partial class Parser
{
    private readonly Context ctx;

    public TokenType Peek => tokens[currentTokenIndex].Type;

    public Parser(Context ctx)
    {
        this.ctx = ctx;
    }

    public Ast Parse(Token[] tokens)
    {
        currentTokenIndex = 0;
        this.tokens = tokens;
        PeekToken = tokens[currentTokenIndex];
        List<FunctionDecleration> functions = new();
        while (true)
        {
            switch (Peek)
            {
                case TokenType.KeywordFunc: functions.Add(ParseFuncDecleration()); break;
                default: return new Ast(functions.ToArray());
            }
        }

        throw new CompilerDebugException("Unreachable");
    }

    private FunctionDecleration ParseFuncDecleration()
    {
        Log.Trace();

        EatToken(TokenType.KeywordFunc);
        string identifier = EatIdentifier();
        EatToken(TokenType.OpenParenthesis);

        List<ParameterDecleration> parameters = new();

        while (Peek != TokenType.CloseParenthesis)
        {
            parameters.Add(new(EatIdentifier(), EatIdentifier()));
        }

        EatToken(TokenType.CloseParenthesis);

        BlockStatement body = ParseBlock();

        return new FunctionDecleration(identifier, parameters.ToArray(), body);
    }

    private BlockStatement ParseBlock()
    {
        Log.Trace();

        EatToken(TokenType.OpenScope);

        List<INode> statements = new();
        while (Peek != TokenType.CloseScope)
        {
            statements.Add((INode)ParseStatement());
        }

        EatToken(TokenType.CloseScope);

        return new BlockStatement(statements.ToArray());
    }

    private INode ParseStatement()
    {
        Log.Trace();

        return Peek switch
        {
            TokenType.Identifier => ParseIdentifier(EatIdentifier()),
            TokenType.KeywordIf => ParseIf(),
            TokenType.KeywordReturn => ParseReturn(),
            _ => throw new NotImplementedException(Peek.ToString())
        };
    }

    private INode ParseIdentifier(string identifier)
    {
        Log.Trace();

        return Peek switch
        {
            TokenType.OpenParenthesis => ParseFuncCall(identifier),
            TokenType.Assignment => ParseVariableDecleration(identifier),
            _ => throw new NotImplementedException()
        };
    }

    private IfStatement ParseIf()
    {
        Log.Trace();

        EatToken(TokenType.KeywordIf);

        INode condition = ParseExpression();

        BlockStatement body = ParseBlock();
        BlockStatement? elseBody = null;

        if (Peek == TokenType.KeywordElse)
        {
            EatToken(TokenType.KeywordElse);
            elseBody = ParseBlock();
        }

        return new IfStatement(condition, body, elseBody);
    }

    /// <summary> RETURN [STATEMENT] </summary>
    private ReturnStatement ParseReturn()
    {
        Log.Trace();

        EatToken(TokenType.KeywordReturn);
        return new ReturnStatement(ParseExpression());
    }

    private INode ParseExpression()
    {
        Log.Trace();

        INode left = ParseUnaryExpression();

        if (IsBinaryToken(Peek))
        {
            Token op = NextToken();
            return new BinaryExpression(left, op.Value, ParseUnaryExpression());
        }
        else
        {
            return left;
        }
    }

    private INode ParseUnaryExpression()
    {
        Token token = NextToken();
        return token.Type switch
        {
            TokenType.Identifier => Peek switch
            {
                TokenType.OpenParenthesis => ParseFuncCall(token.Value),
                _ => new Identifier(token.Value),
            },
            TokenType.IntegerLiteral => new ValueLiteral(ValueType.Int, token.Value),
            TokenType.FloatLiteral => new ValueLiteral(ValueType.Float, token.Value),
            TokenType.StringLiteral => new ValueLiteral(ValueType.String, token.Value),
            _ => new GarbageExpression(token),
        };
    }

    private INode ParseFuncCall(string identifier)
    {
        Log.Trace();

        EatToken(TokenType.OpenParenthesis);

        List<INode> args = new();
        while (Peek != TokenType.CloseParenthesis)
        {
            args.Add(ParseExpression());
        }

        EatToken(TokenType.CloseParenthesis);
        return new FunctionCallExpression(identifier, args.ToArray());
    }

    private INode ParseVariableDecleration(string identifier)
    {
        Log.Trace();

        EatToken(TokenType.Assignment);

        INode rhs = ParseStatement();

        return new AssignmentExpression(new Identifier(identifier), rhs);
    }
}
