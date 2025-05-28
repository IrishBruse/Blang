namespace IBlang.AstParser;

using System.Collections.Generic;
using IBlang.Exceptions;
using IBlang.Tokenizer;
using IBlang.Utility;

public partial class Parser(CompilationData data)
{
    IEnumerator<Token> tokens = null!;

    public CompilationUnit Parse(IEnumerator<Token> tokens, string file)
    {
        this.tokens = tokens;
        tokens.MoveNext();
        CompilationUnit topLevel = ParseTopLevel();
        topLevel.File = file;
        return topLevel;
    }

    CompilationUnit ParseTopLevel()
    {
        List<FunctionStatement> functions = [];

        EatComments();

        while (Peek() == TokenType.Identifier && Peek() != TokenType.Eof && Peek() != TokenType.Garbage)
        {
            EatComments();

            Token identifier = Eat(TokenType.Identifier);

            if (Peek() == TokenType.OpenParenthesis)
            {
                try
                {
                    functions.Add(ParseFunctionDecleration(identifier));
                }
                catch (ParserException e)
                {
                    Error(e);
                }
            }
            else
            {
                Next();
                Error($"Expected '(' or ';' but got {Peek()}");
                break;
            }

            EatComments();
        }

        return new(functions, [])
        {
            Range = new()
        };
    }

    private void EatComments()
    {
        while (Peek() == TokenType.Comment)
        {
            Eat(TokenType.Comment);
        }
    }

    FunctionStatement ParseFunctionDecleration(Token identifier)
    {
        SourceRange begin = identifier.Range;

        List<Expression> parameters = [];

        Eat(TokenType.OpenParenthesis);
        while (Peek() != TokenType.CloseParenthesis && Peek() != TokenType.Eof)
        {
            parameters.Add(ParseExpression());
        }
        Eat(TokenType.CloseParenthesis);

        List<Statement> statements = [];

        Eat(TokenType.OpenScope);
        while (Peek() != TokenType.CloseScope)
        {
            Statement? statement = ParseStatement();

            if (statement != null)
            {
                statements.Add(statement);
            }
            else
            {
                break;
            }
        }
        Token end = Eat(TokenType.CloseScope);

        return new FunctionStatement(identifier, parameters.ToArray(), statements.ToArray())
        {
            Range = begin.Merge(end.Range),
        };
    }

    Statement? ParseStatement()
    {
        return Peek() switch
        {
            TokenType.ExternKeyword => ParseExternalDefinition(),
            TokenType.AutoKeyword => ParseAutoDefinition(),
            TokenType.Identifier => ParseIdentifierStatement(),
            _ => throw new InvalidTokenException("Unexpected Token of type " + Peek())
        };
    }

    ExternalStatement ParseExternalDefinition()
    {
        Token start = Eat(TokenType.ExternKeyword);

        Token identifier = Eat(TokenType.Identifier);

        List<string> externs = [identifier];
        if (Peek() == TokenType.Comma)
        {
            Eat(TokenType.Comma);
            identifier = Eat(TokenType.Identifier);
            externs.Add(identifier);
        }

        Token end = Eat(TokenType.Semicolon);

        return new ExternalStatement(externs.ToArray())
        {
            Range = start.Range.Merge(end.Range),
        };
    }

    AutoStatement ParseAutoDefinition()
    {
        Token start = Eat(TokenType.AutoKeyword);

        Token identifier = Eat(TokenType.Identifier);

        List<string> variables = [identifier];
        if (Peek() == TokenType.Comma)
        {
            Eat(TokenType.Comma);
            identifier = Eat(TokenType.Identifier);
            variables.Add(identifier);
        }

        Token end = Eat(TokenType.Semicolon);

        return new AutoStatement(variables.ToArray())
        {
            Range = start.Range.Merge(end.Range),
        };
    }

    Statement? ParseIdentifierStatement()
    {
        Token identifier = Eat(TokenType.Identifier);

        List<Expression> parameters = [];

        switch (Peek())
        {
            case TokenType.OpenParenthesis:
            {
                Eat(TokenType.OpenParenthesis);
                while (Peek() != TokenType.CloseParenthesis)
                {
                    parameters.Add(ParseExpression());
                    if (Peek() == TokenType.Comma)
                    {
                        Eat(TokenType.Comma);
                    }
                }
                Eat(TokenType.CloseParenthesis);
                Eat(TokenType.Semicolon);

                return new FunctionCall(identifier, parameters.ToArray())
                {
                    Range = identifier.Range,
                };
            }

            case TokenType.Assignment:
            {
                // foo = 12;
                Eat(TokenType.Assignment);
                Token value = Eat(TokenType.IntegerLiteral);
                Eat(TokenType.Semicolon);

                return new VariableAssignment(identifier, value.Content)
                {
                    Range = identifier.Range,
                };
            }

            default: throw new ParserException("Unexpected token in ParseIdentifierStatement of type " + Peek());
        }
    }

    Expression ParseExpression()
    {
        switch (Peek())
        {
            case TokenType.StringLiteral:
            Token str = Eat(TokenType.StringLiteral);

            return new StringValue(str.Content)
            {
                Range = new()
            };

            case TokenType.IntegerLiteral:
            Token integer = Eat(TokenType.IntegerLiteral);

            return new IntValue(int.Parse(integer.Content))
            {
                Range = new()
            };

            default: throw new ParserException("ParseExpression: " + Peek());
        }

    }
}
