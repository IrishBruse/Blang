namespace IBlang;

#pragma warning disable IDE0200,IDE0052

public class AstVisitor
{
    private IVisitor Visitor { get; set; }

    private int indentation;

    private int Indent
    {
        get => indentation;
        set => indentation = Visitor.Indent = value;
    }

    public AstVisitor(IVisitor visitor)
    {
        Visitor = visitor;
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
            Indent++;
            Visit(parameter);
            Indent--;
        }

        Visit(functionDecleration.Body);
    }

    public void Visit(ParameterDefinition parameter)
    {
        Visitor.Visit(parameter);
    }

    public void Visit(IfStatement ifStatement)
    {
        Indent++;
        Visitor.Visit(ifStatement);
        Indent--;
    }

    public void Visit(ReturnStatement statement)
    {
        Indent++;
        Visitor.Visit(statement);
        Indent--;
    }

    public void Visit(AssignmentStatement statement)
    {
        Indent++;
        Visitor.Visit(statement);
        Indent--;

        Indent++;
        Visit(statement.Value);
        Indent--;
    }

    public void Visit(StringLiteral stringLiteral)
    {
        Visitor.Visit(stringLiteral);
    }

    public void Visit(IntegerLiteral integerLiteral)
    {
        Visitor.Visit(integerLiteral);
    }

    public void Visit(BlockBody blockBody)
    {
        Indent++;
        Visitor.Visit(blockBody);
        Indent--;
    }

    public void Visit(Statement statement)
    {
        statement.Switch(
            ifStatement => Visit(ifStatement),
            functionCall => Visit(functionCall),
            returnStatement => Visit(returnStatement),
            assignmentStatement => Visit(assignmentStatement),
            error => Visitor.Visit(error)
        );
    }

    public void Visit(Expression statement)
    {
        statement.Switch(
            stringLiteral => Visit(stringLiteral),
            floatLiteral => Visit(floatLiteral),
            integerLiteral => Visit(integerLiteral),
            identifier => Visit(identifier),
            functionCall => Visit(functionCall),
            error => Visitor.Visit(error)
        );
    }

    public void Visit(Identifier identifier)
    {
        Visitor.Visit(identifier);
    }

    public void Visit(FloatLiteral value)
    {
        Visitor.Visit(value);
    }

    public void Visit(FunctionCallExpression functionCall)
    {
        Visitor.Visit(functionCall);
        foreach (Expression arg in functionCall.Args)
        {
            Visit(arg);
        }
    }
}

#pragma warning restore IDE0200
