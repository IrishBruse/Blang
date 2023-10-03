namespace IBlang;

public record CheckedFunction(string Name, Type ReturnType, CheckedVariable[] Parameters) { }
public record CheckedVariable(string Name, Type Type) { }
