namespace BLang.Ast;

using System.Collections.Generic;

using BLang.Ast.Nodes;
using BLang.Exceptions;
using BLang.Tokenizer;
using BLang.Utility;

#pragma warning disable IDE0072

public partial class Parser(CompilerContext data)
{
    private IEnumerator<Token> tokens = null!;
    private SourceRange previousTokenRange = SourceRange.Zero;
    private readonly SymbolTable symbols = data.Symbols;

    public CompilationUnit Parse(IEnumerator<Token> tokens)
    {
        this.tokens = tokens;

        _ = Next();

        return ParseTopLevel();
    }

    private CompilationUnit ParseTopLevel()
    {
        List<FunctionDecleration> functions = [];
        List<GlobalVariable> globals = [];

        SourceRange start = previousTokenRange;

        EatComments();

        while (Peek(TokenType.Identifier) && !Peek(TokenType.Eof) && !Peek(TokenType.Garbage))
        {
            EatComments();

            Token identifier = Eat(TokenType.Identifier);

            if (Peek(TokenType.OpenParenthesis))
            {
                functions.Add(ParseFunctionDecleration(identifier));
            }
            else if (Peek(TokenType.Semicolon))
            {
                globals.Add(ParseGlobalVariableDecleration(identifier));
            }
            else if (Peek(TokenType.OpenBracket))
            {
                globals.Add(ParseGlobalArrayDecleration(identifier));
            }
            else if (Peek(TokenType.IntegerLiteral))
            {
                globals.Add(ParseGlobalVariableDeclerationInitalizer(identifier));
            }
            else
            {
                throw new ParserException(data.GetFileLocation(previousTokenRange.End) + " Unexpected top level token of type " + Peek());
            }

            EatComments();
        }

        if (Peek(TokenType.Garbage))
        {
            throw new InvalidTokenException(data.GetFileLocation(previousTokenRange.End) + " Garbage token encountered");
        }

        return new(functions.ToArray(), globals.ToArray())
        {
            Range = start.Merge(previousTokenRange)
        };
    }

    private VariableDeclaration ParseGlobalVariableDecleration(Token identifier)
    {
        Symbol symbol = symbols.Add(identifier.Content, SymbolKind.Define);
        _ = Eat(TokenType.Semicolon);

        return new(symbol, null);
    }

    // ('[', Constant?, ']')?, (Ival, (',', Ival)*)?, ';')
    private ArrayDeclaration ParseGlobalArrayDecleration(Token identifier)
    {
        List<int> values = [];

        _ = Eat(TokenType.OpenBracket); // '['
        Token arraySize = Eat(TokenType.IntegerLiteral); // Constant?
        _ = Eat(TokenType.CloseBracket); // ']'

        Symbol symbol = symbols.Add(identifier.Content, SymbolKind.Define);

        // (Ival, (',', Ival)*)?, ';')
        if (Peek(TokenType.IntegerLiteral))
        {
            Token number = Eat(TokenType.IntegerLiteral);
            values.Add(number.Number);

            while (Peek(TokenType.Comma))
            {
                _ = Eat(TokenType.Comma);
                number = Eat(TokenType.IntegerLiteral);
                values.Add(number.Number);
            }
        }
        _ = Eat(TokenType.Semicolon);

        return new(symbol, arraySize.Number, values.ToArray());
    }

    private VariableDeclaration ParseGlobalVariableDeclerationInitalizer(Token identifier)
    {
        Symbol symbol = symbols.Add(identifier.Content, SymbolKind.Define);
        Token number = Eat(TokenType.IntegerLiteral);
        _ = Eat(TokenType.Semicolon);

        return new(symbol, new IntValue(number.ToInteger()));
    }

    private FunctionDecleration ParseFunctionDecleration(Token identifier)
    {
        symbols.EnterScope(identifier.Content);

        Symbol symbol = symbols.Add(identifier.Content, SymbolKind.Define);

        SourceRange begin = identifier.Range;

        List<Variable> parameters = [];

        _ = Eat(TokenType.OpenParenthesis);
        while (!Peek(TokenType.CloseParenthesis) && !Peek(TokenType.Eof))
        {
            Token variable = Eat(TokenType.Identifier);
            symbol = symbols.Add(variable.Content, SymbolKind.Define);
            parameters.Add(symbol);

            if (Peek(TokenType.Comma))
            {
                _ = Eat(TokenType.Comma);
            }
        }
        _ = Eat(TokenType.CloseParenthesis);

        Statement[] body = ParseBlock();

        symbols.ExitScope();
        return new FunctionDecleration(symbol, parameters.ToArray(), body)
        {
            Range = begin.Merge(previousTokenRange),
        };
    }

    private Statement[] ParseBlock()
    {
        List<Statement> statements = [];

        _ = Eat(TokenType.OpenScope);
        while (!Peek(TokenType.CloseScope))
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
        _ = Eat(TokenType.CloseScope);

        return statements.ToArray();
    }

    private Statement ParseStatement()
    {
        return Peek() switch
        {
            TokenType.ExternKeyword => ParseExternalDefinition(),
            TokenType.WhileKeyword => ParseWhileDefinition(),
            TokenType.SwitchKeyword => ParseWhileDefinition(),
            TokenType.IfKeyword => ParseIfDefinition(),
            TokenType.AutoKeyword => ParseAutoStatement(),
            TokenType.Identifier => ParseIdentifierStatement(),
            _ => throw new InvalidTokenException($"{data.GetFileLocation(previousTokenRange.End)} Unexpected token in {nameof(ParseStatement)} of type {Peek()}")
        };
    }

    private WhileStatement ParseWhileDefinition()
    {
        Token start = Eat(TokenType.WhileKeyword);

        Expression condition = ParseBinaryExpression();
        if (condition is not BinaryExpression)
        {
            throw new ParserException("ParseWhileDefinition condition: " + condition);
        }
        Statement[] body = ParseBlock();

        return new((BinaryExpression)condition, body)
        {
            Range = start.Range.Merge(previousTokenRange),
        };
    }

    private SwitchStatement ParseSwitchDefinition()
    {
        Token start = Eat(TokenType.SwitchKeyword);

        Expression condition = ParseBinaryExpression();
        if (condition is not BinaryExpression)
        {
            throw new ParserException("ParseSwitchDefinition condition: " + condition);
        }
        Statement[] body = ParseBlock();

        return new((BinaryExpression)condition, body)
        {
            Range = start.Range.Merge(previousTokenRange),
        };
    }

    private IfStatement ParseIfDefinition()
    {
        Token start = Eat(TokenType.IfKeyword);

        Expression condition = ParseBinaryExpression();
        // if (condition is not BinaryExpression)
        // {
        //     throw new ParserException("ParseWhileDefinition condition: " + condition);
        // }

        Statement[] body = !Peek(TokenType.OpenScope) ? [ParseStatement()] : ParseBlock();
        Statement[]? elseBody = null;
        if (Peek(TokenType.ElseKeyword))
        {
            _ = Eat(TokenType.ElseKeyword);
            elseBody = ParseBlock();
        }

        return new(condition, body, elseBody)
        {
            Range = start.Range.Merge(previousTokenRange),
        };
    }

    private ExternalStatement ParseExternalDefinition()
    {
        Token start = Eat(TokenType.ExternKeyword);
        Token identifier = Eat(TokenType.Identifier);

        List<Symbol> externs = [symbols.Add(identifier, SymbolKind.Define)];
        while (Peek(TokenType.Comma))
        {
            _ = Eat(TokenType.Comma);
            identifier = Eat(TokenType.Identifier);
            externs.Add(symbols.Add(identifier, SymbolKind.Define));
        }

        Token end = Eat(TokenType.Semicolon);

        return new ExternalStatement(externs.ToArray())
        {
            Range = start.Range.Merge(end.Range),
        };
    }

    // ('auto', Name, Constant?, (',', Name, Constant?)*, ';', Statement)
    private AutoStatement ParseAutoStatement()
    {
        List<VariableAssignment> variables = [];
        int value = 0;

        // 'auto'
        Token auto = Eat(TokenType.AutoKeyword);

        // Name
        Token identifier = Eat(TokenType.Identifier);

        // Constant?
        if (TryEat(TokenType.IntegerLiteral, out Token? token))
        {
            value = int.Parse(token!.Content);
        }
        Symbol sym = symbols.Add(identifier, SymbolKind.Define);
        variables.Add(new(sym, value));

        // (',', Name, Constant?)*
        while (Peek(TokenType.Comma))
        {
            value = 0;

            // ','
            _ = Eat(TokenType.Comma);

            // Name
            identifier = Eat(TokenType.Identifier);

            // Constant?
            if (TryEat(TokenType.IntegerLiteral, out token))
            {
                value = int.Parse(token!.Content);
            }

            sym = symbols.Add(identifier, SymbolKind.Define);
            variables.Add(new(sym, value));
        }

        // ';'
        _ = Eat(TokenType.Semicolon);

        // Statement (handled by body)
        return new AutoStatement(variables.ToArray())
        {
            Range = auto.Range.Merge(previousTokenRange),
        };
    }

    private Statement ParseIdentifierStatement()
    {
        Token identifier = Eat(TokenType.Identifier);

        List<Expression> parameters = [];

        if (TryEat(TokenType.OpenParenthesis))
        {
            return ParseFunctionCall(identifier, parameters);
        }
        else if (TryEat(TokenType.Assignment))
        {
            return ParseVariableAssignment(identifier);
        }
        else if (TryEat(TokenType.Increment))
        {
            return ParseVariableAssignmentShorthand(identifier, new IntValue(1));
        }
        else if (TryEat(TokenType.AdditionAssignment))
        {
            return ParseVariableAssignmentShorthand(identifier, ParseBinaryExpression());
        }

        throw new ParserException($"{data.GetFileLocation(previousTokenRange.End)} Unexpected token in {nameof(ParseIdentifierStatement)} of type {Peek()}");
    }

    private FunctionCall ParseFunctionCall(Token identifier, List<Expression> parameters)
    {
        while (!Peek(TokenType.CloseParenthesis))
        {
            parameters.Add(ParseParameter());
            _ = TryEat(TokenType.Comma);
        }
        _ = Eat(TokenType.CloseParenthesis);
        _ = Eat(TokenType.Semicolon);

        Symbol symbol = symbols.GetOrAdd(identifier, SymbolKind.Load);

        return new FunctionCall(symbol, parameters.ToArray())
        {
            Range = identifier.Range,
        };
    }

    private Expression ParseParameter()
    {
        return Peek() switch
        {
            TokenType.StringLiteral => ParseString(),
            TokenType.IntegerLiteral => ParseInteger(),
            TokenType.Subtraction => ParseInteger(TokenType.Subtraction),
            TokenType.Addition => ParseInteger(TokenType.Addition),
            TokenType.Identifier => ParseIdentifier(),
            _ => throw new ParserException($"{data.GetFileLocation(previousTokenRange.End)} ParseExpression: {Peek()}"),
        };
    }

    private Expression ParseIdentifier()
    {
        Token variable = Eat(TokenType.Identifier);
        Symbol? symbol = symbols.GetOrAdd(variable, SymbolKind.Load);
        if (symbol == null)
        {
            string loc = data.GetFileLocation(variable.Range.Start);
            throw new ParserException($"{loc}  {variable}");
        }

        // TODO: match b better (Rvalue, '[', Rvalue, ']')
        if (Peek(TokenType.OpenBracket))
        {
            _ = Eat(TokenType.OpenBracket);
            Expression rval = ParsePrimary();
            _ = Eat(TokenType.CloseBracket);

            // Create a pointer dereference expression
            return new ArrayIndexExpression(new Variable(symbol) { Range = variable.Range }, rval)
            {
                Range = variable.Range
            };
        }

        return new Variable(symbol)
        {
            Range = variable.Range
        };
    }

    private VariableDeclaration ParseVariableAssignment(Token identifier)
    {
        Expression value = ParseBinaryExpression();
        _ = Eat(TokenType.Semicolon);

        Symbol symbol = symbols.GetOrAdd(identifier, SymbolKind.Assign);

        return new VariableDeclaration(symbol, value)
        {
            Range = identifier.Range,
        };
    }

    private VariableDeclaration ParseVariableAssignmentShorthand(Token identifier, Expression shorthandValue)
    {
        Symbol symbol = symbols.GetOrAdd(identifier, SymbolKind.Assign);

        Expression value = new BinaryExpression(BinaryOperator.Addition, new Variable(symbol), shorthandValue);
        _ = Eat(TokenType.Semicolon);

        return new VariableDeclaration(symbol, value)
        {
            Range = identifier.Range,
        };
    }
}
