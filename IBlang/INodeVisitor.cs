namespace IBlang;

public interface INodeVisitor
{
    void Visit(FileAst file);
    void Visit(FunctionDecleration functionDecleration);
    void Visit(ParameterDefinition parameter);
    void Visit(IfStatement ifStatement);
    void Visit(BinaryExpression binaryExpression);
    void Visit(StringLiteral stringLiteral);
    void Visit(FunctionCall functionCall);
    void Visit(IntegerLiteral integerLiteral);
    void Visit(Garbage garbage);
}
