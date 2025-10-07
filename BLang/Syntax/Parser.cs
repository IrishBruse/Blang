namespace BLang.Ast;

using System;
using System.Collections.Generic;

using BLang.Ast.Nodes;
using BLang.Exceptions;
using BLang.Tokenizer;
using BLang.Utility;

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
        List<VariableDeclaration> globals = [];

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
            TokenType.AutoKeyword => ParseAutoDefinition(),
            TokenType.Identifier => ParseIdentifierStatement(),
            TokenType.Eof => throw new NotImplementedException(),
            TokenType.Garbage => throw new NotImplementedException(),
            TokenType.None => throw new NotImplementedException(),
            TokenType.Comment => throw new NotImplementedException(),
            TokenType.IntegerLiteral => throw new NotImplementedException(),
            TokenType.FloatLiteral => throw new NotImplementedException(),
            TokenType.StringLiteral => throw new NotImplementedException(),
            TokenType.CharLiteral => throw new NotImplementedException(),
            TokenType.OpenParenthesis => throw new NotImplementedException(),
            TokenType.CloseParenthesis => throw new NotImplementedException(),
            TokenType.OpenBracket => throw new NotImplementedException(),
            TokenType.CloseBracket => throw new NotImplementedException(),
            TokenType.OpenScope => throw new NotImplementedException(),
            TokenType.CloseScope => throw new NotImplementedException(),
            TokenType.Dot => throw new NotImplementedException(),
            TokenType.Comma => throw new NotImplementedException(),
            TokenType.Addition => throw new NotImplementedException(),
            TokenType.Subtraction => throw new NotImplementedException(),
            TokenType.Multiplication => throw new NotImplementedException(),
            TokenType.Division => throw new NotImplementedException(),
            TokenType.Modulo => throw new NotImplementedException(),
            TokenType.LessThan => throw new NotImplementedException(),
            TokenType.GreaterThan => throw new NotImplementedException(),
            TokenType.LessThanEqual => throw new NotImplementedException(),
            TokenType.GreaterThanEqual => throw new NotImplementedException(),
            TokenType.EqualEqual => throw new NotImplementedException(),
            TokenType.NotEqual => throw new NotImplementedException(),
            TokenType.LogicalAnd => throw new NotImplementedException(),
            TokenType.LogicalOr => throw new NotImplementedException(),
            TokenType.LogicalNot => throw new NotImplementedException(),
            TokenType.Assignment => throw new NotImplementedException(),
            TokenType.AdditionAssignment => throw new NotImplementedException(),
            TokenType.SubtractionAssignment => throw new NotImplementedException(),
            TokenType.MultiplicationAssignment => throw new NotImplementedException(),
            TokenType.DivisionAssignment => throw new NotImplementedException(),
            TokenType.ModuloAssignment => throw new NotImplementedException(),
            TokenType.Increment => throw new NotImplementedException(),
            TokenType.Decrement => throw new NotImplementedException(),
            TokenType.BitwiseComplement => throw new NotImplementedException(),
            TokenType.BitwiseAnd => throw new NotImplementedException(),
            TokenType.BitwiseOr => throw new NotImplementedException(),
            TokenType.BitwiseXOr => throw new NotImplementedException(),
            TokenType.BitwiseShiftLeft => throw new NotImplementedException(),
            TokenType.BitwiseShiftRight => throw new NotImplementedException(),
            TokenType.Semicolon => throw new NotImplementedException(),
            TokenType.ElseKeyword => throw new NotImplementedException(),
            TokenType.CaseKeyword => throw new NotImplementedException(),
            TokenType.BreakKeyword => throw new NotImplementedException(),
            TokenType.ArrayIndexing => throw new NotImplementedException(),
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
        if (condition is not BinaryExpression)
        {
            throw new ParserException("ParseWhileDefinition condition: " + condition);
        }

        Statement[] body = !Peek(TokenType.OpenScope) ? [ParseStatement()] : ParseBlock();
        Statement[]? elseBody = null;
        if (Peek(TokenType.ElseKeyword))
        {
            _ = Eat(TokenType.ElseKeyword);
            elseBody = ParseBlock();
        }

        return new((BinaryExpression)condition, body, elseBody)
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
    private AutoStatement ParseAutoDefinition()
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

        TokenType token = Peek();
        _ = Eat(token);
        return token switch
        {
            TokenType.OpenParenthesis => ParseFunctionCall(identifier, parameters),
            TokenType.Assignment => ParseVariableAssignment(identifier),
            TokenType.Increment => ParseVariableAssignmentShorthand(identifier, new IntValue(1)),
            TokenType.Eof => throw new NotImplementedException(),
            TokenType.Garbage => throw new NotImplementedException(),
            TokenType.None => throw new NotImplementedException(),
            TokenType.Comment => throw new NotImplementedException(),
            TokenType.Identifier => throw new NotImplementedException(),
            TokenType.IntegerLiteral => throw new NotImplementedException(),
            TokenType.FloatLiteral => throw new NotImplementedException(),
            TokenType.StringLiteral => throw new NotImplementedException(),
            TokenType.CharLiteral => throw new NotImplementedException(),
            TokenType.CloseParenthesis => throw new NotImplementedException(),
            TokenType.OpenBracket => throw new NotImplementedException(),
            TokenType.CloseBracket => throw new NotImplementedException(),
            TokenType.OpenScope => throw new NotImplementedException(),
            TokenType.CloseScope => throw new NotImplementedException(),
            TokenType.Dot => throw new NotImplementedException(),
            TokenType.Comma => throw new NotImplementedException(),
            TokenType.Addition => throw new NotImplementedException(),
            TokenType.Subtraction => throw new NotImplementedException(),
            TokenType.Multiplication => throw new NotImplementedException(),
            TokenType.Division => throw new NotImplementedException(),
            TokenType.Modulo => throw new NotImplementedException(),
            TokenType.LessThan => throw new NotImplementedException(),
            TokenType.GreaterThan => throw new NotImplementedException(),
            TokenType.LessThanEqual => throw new NotImplementedException(),
            TokenType.GreaterThanEqual => throw new NotImplementedException(),
            TokenType.EqualEqual => throw new NotImplementedException(),
            TokenType.NotEqual => throw new NotImplementedException(),
            TokenType.LogicalAnd => throw new NotImplementedException(),
            TokenType.LogicalOr => throw new NotImplementedException(),
            TokenType.LogicalNot => throw new NotImplementedException(),
            TokenType.AdditionAssignment => ParseVariableAssignmentShorthand(identifier, ParseBinaryExpression()),
            TokenType.SubtractionAssignment => throw new NotImplementedException(),
            TokenType.MultiplicationAssignment => throw new NotImplementedException(),
            TokenType.DivisionAssignment => throw new NotImplementedException(),
            TokenType.ModuloAssignment => throw new NotImplementedException(),
            TokenType.Decrement => throw new NotImplementedException(),
            TokenType.BitwiseComplement => throw new NotImplementedException(),
            TokenType.BitwiseAnd => throw new NotImplementedException(),
            TokenType.BitwiseOr => throw new NotImplementedException(),
            TokenType.BitwiseXOr => throw new NotImplementedException(),
            TokenType.BitwiseShiftLeft => throw new NotImplementedException(),
            TokenType.BitwiseShiftRight => throw new NotImplementedException(),
            TokenType.Semicolon => throw new NotImplementedException(),
            TokenType.ExternKeyword => throw new NotImplementedException(),
            TokenType.IfKeyword => throw new NotImplementedException(),
            TokenType.ElseKeyword => throw new NotImplementedException(),
            TokenType.WhileKeyword => throw new NotImplementedException(),
            TokenType.AutoKeyword => throw new NotImplementedException(),
            TokenType.SwitchKeyword => throw new NotImplementedException(),
            TokenType.CaseKeyword => throw new NotImplementedException(),
            TokenType.BreakKeyword => throw new NotImplementedException(),
            TokenType.ArrayIndexing => throw new NotImplementedException(),
            _ => throw new ParserException($"{data.GetFileLocation(previousTokenRange.End)} Unexpected token in {nameof(ParseIdentifierStatement)} of type {Peek()}"),
        };
    }

    private FunctionCall ParseFunctionCall(Token identifier, List<Expression> parameters)
    {
        while (!Peek(TokenType.CloseParenthesis))
        {
            parameters.Add(ParseParameter());
            if (Peek(TokenType.Comma))
            {
                _ = Eat(TokenType.Comma);
            }
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
            TokenType.Identifier => ParseIdentifier(),
            TokenType.Eof => throw new NotImplementedException("TokenType.Eof"),
            TokenType.Garbage => throw new NotImplementedException("TokenType.Garbage"),
            TokenType.None => throw new NotImplementedException("TokenType.None"),
            TokenType.Comment => throw new NotImplementedException("TokenType.Comment"),
            TokenType.FloatLiteral => throw new NotImplementedException("TokenType.FloatLiteral"),
            TokenType.CharLiteral => throw new NotImplementedException("TokenType.CharLiteral"),
            TokenType.OpenParenthesis => throw new NotImplementedException("TokenType.OpenParenthesis"),
            TokenType.CloseParenthesis => throw new NotImplementedException("TokenType.CloseParenthesis"),
            TokenType.OpenBracket => throw new NotImplementedException("TokenType.OpenBracket"),
            TokenType.CloseBracket => throw new NotImplementedException("TokenType.CloseBracket"),
            TokenType.OpenScope => throw new NotImplementedException("TokenType.OpenScope"),
            TokenType.CloseScope => throw new NotImplementedException("TokenType.CloseScope"),
            TokenType.Dot => throw new NotImplementedException("TokenType.Dot"),
            TokenType.Comma => throw new NotImplementedException("TokenType.Comma"),
            TokenType.Addition => throw new NotImplementedException("TokenType.Addition"),
            TokenType.Subtraction => ParseInteger(TokenType.Subtraction),
            TokenType.Multiplication => throw new NotImplementedException("TokenType.Multiplication"),
            TokenType.Division => throw new NotImplementedException("TokenType.Division"),
            TokenType.Modulo => throw new NotImplementedException("TokenType.Modulo"),
            TokenType.LessThan => throw new NotImplementedException("TokenType.LessThan"),
            TokenType.GreaterThan => throw new NotImplementedException("TokenType.GreaterThan"),
            TokenType.LessThanEqual => throw new NotImplementedException("TokenType.LessThanEqual"),
            TokenType.GreaterThanEqual => throw new NotImplementedException("TokenType.GreaterThanEqual"),
            TokenType.EqualEqual => throw new NotImplementedException("TokenType.EqualEqual"),
            TokenType.NotEqual => throw new NotImplementedException("TokenType.NotEqual"),
            TokenType.LogicalAnd => throw new NotImplementedException("TokenType.LogicalAnd"),
            TokenType.LogicalOr => throw new NotImplementedException("TokenType.LogicalOr"),
            TokenType.LogicalNot => throw new NotImplementedException("TokenType.LogicalNot"),
            TokenType.Assignment => throw new NotImplementedException("TokenType.Assignment"),
            TokenType.AdditionAssignment => throw new NotImplementedException("TokenType.AdditionAssignment"),
            TokenType.SubtractionAssignment => throw new NotImplementedException("TokenType.SubtractionAssignment"),
            TokenType.MultiplicationAssignment => throw new NotImplementedException("TokenType.MultiplicationAssignment"),
            TokenType.DivisionAssignment => throw new NotImplementedException("TokenType.DivisionAssignment"),
            TokenType.ModuloAssignment => throw new NotImplementedException("TokenType.ModuloAssignment"),
            TokenType.Increment => throw new NotImplementedException("TokenType.Increment"),
            TokenType.Decrement => throw new NotImplementedException("TokenType.Decrement"),
            TokenType.BitwiseComplement => throw new NotImplementedException("TokenType.BitwiseComplement"),
            TokenType.BitwiseAnd => throw new NotImplementedException("TokenType.BitwiseAnd"),
            TokenType.BitwiseOr => throw new NotImplementedException("TokenType.BitwiseOr"),
            TokenType.BitwiseXOr => throw new NotImplementedException("TokenType.BitwiseXOr"),
            TokenType.BitwiseShiftLeft => throw new NotImplementedException("TokenType.BitwiseShiftLeft"),
            TokenType.BitwiseShiftRight => throw new NotImplementedException("TokenType.BitwiseShiftRight"),
            TokenType.Semicolon => throw new NotImplementedException("TokenType.Semicolon"),
            TokenType.ExternKeyword => throw new NotImplementedException("TokenType.ExternKeyword"),
            TokenType.IfKeyword => throw new NotImplementedException("TokenType.IfKeyword"),
            TokenType.ElseKeyword => throw new NotImplementedException("TokenType.ElseKeyword"),
            TokenType.WhileKeyword => throw new NotImplementedException("TokenType.WhileKeyword"),
            TokenType.AutoKeyword => throw new NotImplementedException("TokenType.AutoKeyword"),
            TokenType.SwitchKeyword => throw new NotImplementedException("TokenType.SwitchKeyword"),
            TokenType.CaseKeyword => throw new NotImplementedException("TokenType.CaseKeyword"),
            TokenType.BreakKeyword => throw new NotImplementedException("TokenType.BreakKeyword"),
            TokenType.ArrayIndexing => throw new NotImplementedException("TokenType.ArrayIndexing"),
            _ => throw new ParserException($"{data.GetFileLocation(previousTokenRange.End)} ParseExpression: {Peek()}"),
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
