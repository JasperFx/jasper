using System;

namespace Jasper.Persistence.Sagas;

public class InvalidSagaException : Exception
{
    public InvalidSagaException(string? message) : base(message)
    {
    }

    public InvalidSagaException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}


