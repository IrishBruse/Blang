namespace IBlang;

public interface INodeVisitor
{
    void Visit(FileAst file);
    void Visit(FunctionDecleration functionDecleration);
    void Visit(Parameter parameter);
    void Visit(Statement statement);
    void Visit(IfStatement ifStatement);
    void Visit(BinaryExpression binaryExpression);
    void Visit(StringLiteral stringLiteral);
    void Visit(FunctionCall functionCall);
}
