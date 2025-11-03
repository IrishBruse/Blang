namespace BLang.Ast;

using System;
using System.Collections.Generic;

using BLang.Ast.Nodes;
using BLang.Exceptions;
using BLang.Tokenizer;
using BLang.Utility;

public partial class Parser(CompilerContext data)
{
    public SourceRange TokenPosition { get; set; } = SourceRange.Zero;

    private IEnumerator<Token> tokens = null!;
    private readonly SymbolTable symbols = data.Symbols;

    public Result<CompilationUnit> Parse(IEnumerator<Token> tokens)
    {
        this.tokens = tokens;

        _ = Next();

        try
        {
            return ParseCompilationUnit();
        }
        catch (Exception e)
        {
            string error = Options.Verbose > 0 ? e.ToString() : e.Message;
            return data.GetFileLocation(TokenPosition.End) + " " + error.Trim();
        }
    }

    // CompilationUnit
    //     Definition*
    //
    // Definition
    //     GlobalVariableDecleration
    //     FunctionDecleration
    private CompilationUnit ParseCompilationUnit()
    {
        List<FunctionDecleration> functions = [];
        List<GlobalVariable> globals = [];

        SourceRange start = TokenPosition;

        while (Peek(TokenType.Identifier) && !Peek(TokenType.Eof) && !Peek(TokenType.Garbage))
        {
            Token identifier = Eat(TokenType.Identifier);

            // Definition
            //     FunctionDecleration
            //     GlobalVariableDecleration
            if (Peek(TokenType.OpenParenthesis))
            {
                functions.Add(ParseFunctionDecleration(identifier));
            }
            else
            {
                globals.Add(ParseGlobalVariableDecleration(identifier));
            }

            EatComments();
        }

        if (Peek(TokenType.Garbage))
        {
            throw new InvalidTokenException("Garbage token encountered");
        }

        return new(functions.ToArray(), globals.ToArray())
        {
            Range = Range(start, TokenPosition),
        };
    }

    // FunctionDecleration
    //     (Identifier, '(', (Identifier, (',', Identifier)*)?, ')', Block)
    private FunctionDecleration ParseFunctionDecleration(Token identifier)
    {
        symbols.EnterScope(identifier.Content);
        Symbol symbol = symbols.Add(identifier.Content);

        SourceRange start = identifier.Range;

        List<Variable> parameters = [];

        _ = Eat(TokenType.OpenParenthesis);

        // (Identifier, (',', Identifier)*)?
        if (Peek(TokenType.Identifier))
        {
            parameters.Add(EatSymbol());

            // (',', Identifier)*
            while (TryEat(TokenType.Comma))
            {
                parameters.Add(EatSymbol());
            }
        }

        _ = Eat(TokenType.CloseParenthesis);

        Statement[] body = ParseBlock();

        symbols.ExitScope();
        return new FunctionDecleration(symbol, parameters.ToArray(), body)
        {
            Range = Range(start, TokenPosition),
        };
    }

    // GlobalVariableDecleration
    //     (Identifier, ('[', Constant?, ']')?, (Ival, (',', Ival)*)?, ';')
    private GlobalVariable ParseGlobalVariableDecleration(Token identifier)
    {
        // Identifier,
        Symbol symbol = symbols.Add(identifier.Content);

        int? arraySize = null;
        bool isArray = false;

        // ('[', Constant?, ']')?,
        if (TryEat(TokenType.OpenBracket)) // '['
        {
            isArray = true;

            // Constant?
            if (TryEat(TokenType.IntegerLiteral, out Token? token))
            {
                arraySize = token.Number;
            }
            _ = Eat(TokenType.CloseBracket); // ']'
        }

        List<Expression> values = new();

        int valueCount = 0;

        // (Ival, (',', Ival)*)?,
        if (Peek(TokenType.IntegerLiteral))
        {
            values.Add(new IntValue(EatInt()));
            valueCount++;

            while (TryEat(TokenType.Comma))
            {
                values.Add(new IntValue(EatInt()));
                valueCount++;
            }

            if (arraySize != null && arraySize < valueCount)
            {
                throw new ParserException($"Array of size {arraySize} contains {valueCount} elements");
            }
        }

        _ = Eat(TokenType.Semicolon);

        if (isArray)
        {
            int size = valueCount;
            if (valueCount < arraySize)
            {
                size = arraySize.Value;
            }
            return new GlobalArrayDeclaration(symbol, values.ToArray(), size) { Range = Range(identifier, TokenPosition) };
        }
        else
        {
            return new GlobalVariableDecleration(symbol, valueCount == 1 ? values[0] : null) { Range = Range(identifier, TokenPosition) };
        }
    }

    // Block
    //     ('{', Statement, '}')
    //     Statement
    private Statement[] ParseBlock()
    {
        List<Statement> statements = [];

        //     ('{', Statement, '}')
        if (TryEat(TokenType.OpenScope))
        {
            while (!Peek(TokenType.CloseScope))
            {
                statements.Add(ParseStatement());
            }
            _ = Eat(TokenType.CloseScope);
        }
        else //     Statement
        {
            statements.Add(ParseStatement());
        }

        return statements.ToArray();
    }

    // Statement
    //     AutoStatement
    //     ExternStatement
    //     IfStatement
    //     WhileStatement
    //     SwitchStatement
    //     CaseStatement
    //     GotoStatement
    //     LabelStatement
    //     ReturnStatement
    //     (Rvalue?, ';')
    private Statement ParseStatement()
    {
        return Peek() switch
        {
            TokenType.AutoKeyword => ParseAutoStatement(),
            TokenType.ExternKeyword => ParseExternalDefinition(),
            TokenType.IfKeyword => ParseIfDefinition(),
            TokenType.WhileKeyword => ParseWhileDefinition(),
            // TODO: Switch
            // TODO: Case
            // TODO: Goto
            // TODO: Label
            // TODO: Return
            TokenType.Identifier => ParseIdentifierStatement(),
            _ => throw new InvalidTokenException($"Unexpected token in {nameof(ParseStatement)} of type {Peek()}")
        };
    }


    // AutoStatement
    //     ('auto', Identifier, Constant?, (',', Identifier, Constant?)*, ';')
    private AutoStatement ParseAutoStatement()
    {
        List<VariableAssignment> variables = [];
        int value = 0;

        // 'auto'
        Token start = Eat(TokenType.AutoKeyword);

        // Identifier
        Token identifier = Eat(TokenType.Identifier);

        // Constant?
        if (TryEat(TokenType.IntegerLiteral, out Token? token))
        {
            value = int.Parse(token!.Content);
        }
        Symbol sym = symbols.Add(identifier);
        variables.Add(new(sym, value));

        // (',', Identifier, Constant?)*
        while (Peek(TokenType.Comma))
        {
            value = 0;

            // ','
            _ = Eat(TokenType.Comma);

            // Identifier
            identifier = Eat(TokenType.Identifier);

            // Constant?
            if (TryEat(TokenType.IntegerLiteral, out token))
            {
                value = int.Parse(token!.Content);
            }

            sym = symbols.Add(identifier);
            variables.Add(new(sym, value));
        }

        // ';'
        Token end = Eat(TokenType.Semicolon);

        // Statement (handled by body)
        return new AutoStatement(variables.ToArray())
        {
            Range = Range(start, end),
        };
    }

    // ExternStatement
    //     ('extrn', Identifier, (',', Identifier)*, ';')
    private ExternalStatement ParseExternalDefinition()
    {
        Token start = Eat(TokenType.ExternKeyword);

        Token identifier = Eat(TokenType.Identifier);

        List<Symbol> externs = [symbols.Add(identifier)];
        while (TryEat(TokenType.Comma))
        {
            identifier = Eat(TokenType.Identifier);
            externs.Add(symbols.Add(identifier));
        }

        Token end = Eat(TokenType.Semicolon);

        return new ExternalStatement(externs.ToArray())
        {
            Range = Range(start, end),
        };
    }

    // IfStatement
    //     ('if', '(', Rvalue, ')', Block, ('else', Block)?)
    private IfStatement ParseIfDefinition()
    {
        // 'if'
        Token start = Eat(TokenType.IfKeyword);

        // '(', Rvalue, ')',
        _ = Eat(TokenType.OpenParenthesis);
        Expression condition = ParseRValue();
        _ = Eat(TokenType.CloseParenthesis);

        // Block
        Statement[] body = ParseBlock();

        Statement[]? elseBody = null;

        // ('else', Block)?
        if (TryEat(TokenType.ElseKeyword))
        {
            elseBody = ParseBlock();
        }

        return new(condition, body, elseBody)
        {
            Range = Range(start, TokenPosition),
        };
    }

    // WhileStatement
    //     ('while', '(', Rvalue, ')', Block)
    private WhileStatement ParseWhileDefinition()
    {
        // 'while'
        Token start = Eat(TokenType.WhileKeyword);

        // TODO: '(', Rvalue, ')',
        _ = Eat(TokenType.OpenParenthesis);
        Expression condition = ParseRValue();
        _ = Eat(TokenType.CloseParenthesis);

        // Block
        Statement[] body = ParseBlock();

        return new((BinaryExpression)condition, body)
        {
            Range = Range(start.Range, TokenPosition),
        };
    }

    // SwitchStatement
    //     ('switch', Rvalue, Block)
    private SwitchStatement ParseSwitchDefinition()
    {
        // 'switch'
        Token start = Eat(TokenType.SwitchKeyword);

        // Rvalue
        Expression condition = ParseRValue();

        // Block
        Statement[] body = ParseBlock();

        return new((BinaryExpression)condition, body)
        {
            Range = Range(start, TokenPosition),
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
        else if (TryEat(TokenType.OpenBracket))
        {
            Expression indexExpr = ParseBinaryExpression();
            _ = Eat(TokenType.CloseBracket);

            Symbol symbol = symbols.GetOrAdd(identifier);
            Expression value;

            value = ParseRValueOld(symbol);

            _ = Eat(TokenType.Semicolon);

            return new ArrayAssignmentStatement(
                symbol,
                indexExpr,
                value
            )
            { Range = Range(identifier, TokenPosition), };
        }
        else
        {
            return ParseAssignment(identifier);
        }

        throw new ParserException($"Unexpected token in {nameof(ParseIdentifierStatement)} of type {Peek()}");
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

        Symbol symbol = symbols.GetOrAdd(identifier);

        return new FunctionCall(symbol, parameters.ToArray())
        {
            Range = Range(identifier, TokenPosition),
        };
    }

    private Expression ParseParameter()
    {
        return Peek() switch
        {
            TokenType.StringLiteral => ParseString(),

            TokenType.Subtraction => ParseInteger(TokenType.Subtraction),
            TokenType.Addition => ParseInteger(TokenType.Addition),

            TokenType.IntegerLiteral => ParseBinaryExpression(),
            TokenType.Identifier => ParseBinaryExpression(),
            _ => throw new ParserException(Peek().ToString()),
        };
    }

    private Expression ParseIdentifier()
    {
        Token variable = Eat(TokenType.Identifier);
        Symbol symbol = symbols.GetOrAdd(variable);

        // TODO: match b better (Rvalue, '[', Rvalue, ']')
        if (Peek(TokenType.OpenBracket))
        {
            _ = Eat(TokenType.OpenBracket);
            Expression rval = ParsePrimary();
            _ = Eat(TokenType.CloseBracket);

            // Create a pointer dereference expression
            return new ArrayIndexExpression(new Variable(symbol) { Range = variable.Range }, rval)
            {
                Range = Range(variable, TokenPosition)
            };
        }

        return new Variable(symbol)
        {
            Range = Range(variable, TokenPosition)
        };
    }

    private GlobalVariableDecleration ParseAssignment(Token identifier)
    {
        Symbol symbol = symbols.GetOrAdd(identifier);
        Expression value = ParseRValueOld(symbol);

        _ = Eat(TokenType.Semicolon);

        return new GlobalVariableDecleration(symbol, value) { Range = identifier.Range };
    }

    private Expression ParseRValueOld(Symbol symbol)
    {
        TokenType assignmentType = Peek();
        _ = Eat(assignmentType);
        SourceRange start = TokenPosition;

        // Helper to create left side variable expression
        Variable leftVar = new(symbol);
        Expression value = assignmentType switch
        {
            // Simple assignment: x = <expr>
            TokenType.Assignment => ParseBinaryExpression(),

            // Compound assignments map to the corresponding binary operation:
            // x += y  -> x + y
            TokenType.AdditionAssignment => new BinaryExpression(BinaryOperator.Addition, leftVar, ParseBinaryExpression()),
            // x -= y -> x - y
            TokenType.SubtractionAssignment => new BinaryExpression(BinaryOperator.Subtraction, leftVar, ParseBinaryExpression()),
            // x *= y -> x * y
            TokenType.MultiplicationAssignment => new BinaryExpression(BinaryOperator.Multiplication, leftVar, ParseBinaryExpression()),
            // x /= y -> x / y
            TokenType.DivisionAssignment => new BinaryExpression(BinaryOperator.Division, leftVar, ParseBinaryExpression()),
            // x %= y -> x % y
            TokenType.ModuloAssignment => new BinaryExpression(BinaryOperator.Modulo, leftVar, ParseBinaryExpression()),
            // x <<= y -> x << y
            TokenType.BitwiseShiftLeftAssignment => new BinaryExpression(BinaryOperator.BitwiseShiftLeft, leftVar, ParseBinaryExpression()),
            // x >>= y -> x << y
            TokenType.BitwiseShiftRightAssignment => new BinaryExpression(BinaryOperator.BitwiseShiftRight, leftVar, ParseBinaryExpression()),
            // x |= y -> x << y
            TokenType.BitwiseOrAssignment => new BinaryExpression(BinaryOperator.BitwiseOr, leftVar, ParseBinaryExpression()),

            // Postfix increment / decrement: x++  /  x--
            TokenType.Increment => new BinaryExpression(BinaryOperator.Addition, leftVar, new IntValue(1)),
            TokenType.Decrement => new BinaryExpression(BinaryOperator.Subtraction, leftVar, new IntValue(1)),

            _ => throw new ParserException("Unknown assignment/operation type " + assignmentType),
        };

        return value with { Range = Range(start, TokenPosition) };
    }

    // Rvalue
    //     ('(', Rvalue, ')')
    //     Lvalue
    //     Constant
    //     (Lvalue, Assign, Rvalue)
    //     (IncDec, Lvalue)
    //     (Lvalue, IncDec)
    //     (Unary, Rvalue)
    //     ('&', Lvalue)
    //     (Rvalue, Binary, Rvalue)
    //     (Rvalue, '?', Rvalue, ':', Rvalue)
    //     (Rvalue, '(', (Rvalue, (',', Rvalue)* )?, ')')
    private Expression ParseRValue()
    {
        if (Peek(TokenType.Increment) || Peek(TokenType.Decrement))
        {
            TokenType op = Peek();
            _ = Eat(op);

            Expression left = ParseIdentifier();

            return op == TokenType.Increment
                ? new BinaryExpression(BinaryOperator.Addition, left, new IntValue(1)) { Range = Range(left.Range, TokenPosition) }
                : new BinaryExpression(BinaryOperator.Subtraction, left, new IntValue(1)) { Range = Range(left.Range, TokenPosition) };
        }

        Expression expr = ParseBinaryExpression();

        // Postfix increment/decrement: x++ / x--
        if (Peek(TokenType.Increment) || Peek(TokenType.Decrement))
        {
            TokenType op = Peek();
            _ = Eat(op);

            return op == TokenType.Increment
                ? new BinaryExpression(BinaryOperator.Addition, expr, new IntValue(1)) { Range = Range(expr.Range, TokenPosition) }
                : new BinaryExpression(BinaryOperator.Subtraction, expr, new IntValue(1)) { Range = Range(expr.Range, TokenPosition) };
        }

        return expr;
    }
}
