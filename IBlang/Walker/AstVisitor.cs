namespace IBlang.Walker;

#pragma warning disable IDE0200, IDE0052

public class AstVisitor(IVisitor visitor)
{
    IVisitor Visitor { get; set; } = visitor;

    void Indent()
    {
        Visitor.Indent++;
    }

    void Dedent()
    {
        Visitor.Indent--;
    }

    public void Visit(FileAst file)
    {
        Visitor.Visit(file);

        foreach (FunctionDecleration function in file.Functions)
        {
            Visit(function);
        }
    }

    public void Visit(FunctionDecleration functionDecleration)
    {
        Visitor.Visit(functionDecleration);
        foreach (ParameterDefinition parameter in functionDecleration.Parameters)
        {
            Indent();
            Visit(parameter);
            Dedent();
        }

        Visit(functionDecleration.Body);
    }

    public void Visit(ParameterDefinition parameter)
    {
        Indent();
        Visitor.Visit(parameter);
        Dedent();
    }

    public void Visit(IfStatement ifStatement)
    {
        Indent();
        Visitor.Visit(ifStatement);
        Visit(ifStatement.Condition);
        Dedent();

        Visit(ifStatement.Body);
        if (ifStatement.ElseBody != null)
        {
            Visit(ifStatement.ElseBody);
        }
    }

    public void Visit(ReturnStatement statement)
    {
        Indent();
        Visitor.Visit(statement);
        VisitExpression(statement.Result);
        Dedent();
    }

    public void Visit(AssignmentStatement statement)
    {
        Indent();
        Visitor.Visit(statement);
        VisitExpression(statement.Value);
        Dedent();
    }

    public void Visit(StringLiteral stringLiteral)
    {
        Indent();
        Visitor.Visit(stringLiteral);
        Dedent();
    }

    public void Visit(IntegerLiteral integerLiteral)
    {
        Indent();
        Visitor.Visit(integerLiteral);
        Dedent();
    }

    public void Visit(BlockBody block)
    {
        foreach (Statement statement in block.Statements)
        {
            VisitStatement(statement);
        }
    }

    public void VisitStatement(Statement statement)
    {
        statement.Switch(
            ifStatement => Visit(ifStatement),
            functionCall => Visit(functionCall),
            returnStatement => Visit(returnStatement),
            assignmentStatement => Visit(assignmentStatement),
            error => Visit(error)
        );
    }

    public void VisitExpression(Expression expression)
    {
        expression.Switch(
            stringLiteral => Visit(stringLiteral),
            floatLiteral => Visit(floatLiteral),
            integerLiteral => Visit(integerLiteral),
            identifier => Visit(identifier),
            binaryExpression => Visit(binaryExpression),
            booleanExpression => Visit(booleanExpression),
            functionCall => Visit(functionCall),
            error => Visit(error)
        );
    }

    public void Visit(Identifier identifier)
    {
        Indent();
        Visitor.Visit(identifier);
        Dedent();
    }

    public void Visit(FloatLiteral value)
    {
        Indent();
        Visitor.Visit(value);
        Dedent();
    }

    public void Visit(Data.Token token)
    {
        Indent();
        Visitor.Visit(token);
        Dedent();
    }

    public void Visit(FunctionCallExpression functionCall)
    {
        Indent();
        Visitor.Visit(functionCall);
        foreach (Expression arg in functionCall.Args)
        {
            VisitExpression(arg);
        }
        Dedent();
    }

    public void Visit(BinaryExpression node)
    {
        Indent();
        Visitor.Visit(node);
        Dedent();
    }

    public void Visit(BooleanExpression node)
    {
        Indent();
        Visitor.Visit(node);
        Dedent();
    }

    public void Visit(Error error)
    {
        Indent();
        Visitor.Visit(error);
        Dedent();
    }
}

#pragma warning restore IDE0200
