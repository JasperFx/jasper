using System;

namespace Jasper.Runtime.Routing;

public class UnknownEndpointException : Exception
{
    public UnknownEndpointException(string? message) : base(message)
    {
    }
}