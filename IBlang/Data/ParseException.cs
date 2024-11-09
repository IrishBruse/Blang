namespace IBlang.Data;

using System;
using System.Runtime.Serialization;

[Serializable]
public sealed class ParseException : Exception
{
    public ParseError Error { get; set; }

    public ParseException(ParseError error) : base(error.Message)
    {
        Error = error;
    }

    ParseException(SerializationInfo serializationInfo, StreamingContext streamingContext)
    {
        throw new NotImplementedException();
    }
}
