namespace IBlang;

using System;
using System.Runtime.Serialization;

[Serializable]
public sealed class ParseException : Exception
{
    public ParseException()
    {
    }

    public ParseException(string message) : base(message)
    {
    }

    public ParseException(string message, Exception innerException) : base(message, innerException)
    {
    }

    private ParseException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
