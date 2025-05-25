namespace IBlang.AstParser;

using System;
using System.Collections.Generic;
using IBlang.Exceptions;
using IBlang.Tokenizer;


public partial class Parser
{
    IEnumerator<Token> tokens = null!;

    public CompilationUnit Parse(IEnumerator<Token> tokens)
    {
        this.tokens = tokens;
        tokens.MoveNext();
        return ParseTopLevel();
    }

    CompilationUnit ParseTopLevel()
    {
        List<FunctionDeclaration> functions = [];

        Token identifier = Eat(TokenType.Identifier);

        if (Peek() == TokenType.OpenParenthesis)
        {
            functions.Add(ParseFunctionDecleration(identifier));
        }

        return new(functions, [])
        {
            Range = new()
        };
    }

    FunctionDeclaration ParseFunctionDecleration(Token identifier)
    {
        SourceRange begin = identifier.Range;

        Eat(TokenType.OpenParenthesis);
        List<Expression> parameters = [];
        while (Peek() != TokenType.CloseParenthesis)
        {
            parameters.Add(ParseExpression());
        }
        Eat(TokenType.CloseParenthesis);

        Eat(TokenType.OpenScope);
        List<Statement> statements = [];
        while (Peek() != TokenType.CloseScope)
        {
            statements.Add(ParseStatement());
        }
        Token end = Eat(TokenType.CloseScope);

        return new FunctionDeclaration(identifier, parameters.ToArray(), statements.ToArray())
        {
            Range = begin.Merge(end.Range),
        };
    }

    Statement ParseStatement()
    {
        return Peek() switch
        {
            TokenType.ExternKeyword => ParseExternalDefinition(),
            TokenType.Identifier => ParseIdentifierStatement(),
            _ => throw new InvalidTokenException("Unexpected Token of type " + Peek())
        };
    }

    ExternalStatement ParseExternalDefinition()
    {
        Token start = Eat(TokenType.ExternKeyword);

        // TODO: handle multiple externals and commas
        Token identifier = Eat(TokenType.Identifier);
        Token end = Eat(TokenType.Semicolon);

        return new ExternalStatement(identifier)
        {
            Range = start.Range.Merge(end.Range),
        };
    }

    FunctionCall ParseIdentifierStatement()
    {
        Token identifier = Eat(TokenType.Identifier);

        List<Expression> parameters = [];

        switch (Peek())
        {
            case TokenType.OpenParenthesis:

            // TODO: parse more values
            Eat(TokenType.OpenParenthesis);
            parameters.Add(ParseExpression());
            Eat(TokenType.CloseParenthesis);
            Eat(TokenType.Semicolon);

            return new FunctionCall(identifier, parameters.ToArray())
            {
                Range = identifier.Range,
            };

            default:
            throw new NotImplementedException();
        }
    }

    Expression ParseExpression()
    {
        Token str = Eat(TokenType.StringLiteral);

        return new StringValue(str.Content)
        {
            Range = new()
        };
    }
}
