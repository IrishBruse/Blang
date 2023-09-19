namespace IBlang;

#pragma warning disable IDE0200,IDE0052

public class AstVisitor
{
    IVisitor Visitor { get; set; }

    void Indent()
    {
        Visitor.Indent++;
    }

    void Dedent()
    {
        Visitor.Indent--;
    }

    public AstVisitor(IVisitor visitor)
    {
        Visitor = visitor;
    }

    public void Visit(FileAst file)
    {
        Indent();
        Visitor.Visit(file);

        foreach (FunctionDecleration function in file.Functions)
        {
            Visit(function);
        }
        Dedent();
    }

    public void Visit(FunctionDecleration functionDecleration)
    {
        Indent();
        Visitor.Visit(functionDecleration);
        Dedent();

        foreach (ParameterDefinition parameter in functionDecleration.Parameters)
        {
            Indent();
            Visit(parameter);
            Dedent();
        }

        Indent();
        Visit(functionDecleration.Body);
        Dedent();
    }

    public void Visit(ParameterDefinition parameter)
    {
        Indent();
        {
            Visitor.Visit(parameter);
        }
        Dedent();
    }

    public void Visit(IfStatement ifStatement)
    {
        Indent();
        {
            Visitor.Visit(ifStatement);
        }
        Dedent();
    }

    public void Visit(ReturnStatement statement)
    {
        Indent();
        {
            Visitor.Visit(statement);
        }
        Dedent();
    }

    public void Visit(AssignmentStatement statement)
    {
        Indent();
        {
            Visitor.Visit(statement);

            Indent();
            {
                Visit(statement.Value);
            }
            Dedent();
        }
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

    public void Visit(BlockBody blockBody)
    {
        Indent();
        Visitor.Visit(blockBody);
        Dedent();
    }

    public void Visit(Statement statement)
    {
        statement.Switch(
            ifStatement => Visit(ifStatement),
            functionCall => Visit(functionCall),
            returnStatement => Visit(returnStatement),
            assignmentStatement => Visit(assignmentStatement),
            error => Visit(error)
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

    public void Visit(FunctionCallExpression functionCall)
    {
        Indent();
        Visitor.Visit(functionCall);
        foreach (Expression arg in functionCall.Args)
        {
            Indent();
            Visit(arg);
            Dedent();
        }
        Dedent();
    }

    public void Visit(OneOf.Types.Error<string> error)
    {
        Indent();
        Visitor.Visit(error);
        Dedent();
    }
}

#pragma warning restore IDE0200
