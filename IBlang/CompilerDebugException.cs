namespace IBlang;

using System.Runtime.Serialization;

[Serializable]
internal class CompilerDebugException : Exception
{
    public CompilerDebugException()
    {
    }

    public CompilerDebugException(string? message) : base(message)
    {
    }

    public CompilerDebugException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected CompilerDebugException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
