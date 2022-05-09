using System;

namespace Jasper.Transports.Sending;

public class QueueDoesNotExistException : Exception
{
    public QueueDoesNotExistException(OutgoingMessageBatch outgoing) : base(
        $"Queue '{outgoing.Destination}' does not exist at {outgoing.Destination}")
    {
    }
}
