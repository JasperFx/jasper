using System;
using Jasper.Messaging.Transports.Tcp;
using Jasper.Util;

namespace Jasper.Messaging.Transports.Sending
{
    public class QueueDoesNotExistException : Exception
    {
        public QueueDoesNotExistException(OutgoingMessageBatch outgoing) : base(
            $"Queue '{outgoing.Destination}' does not exist at {outgoing.Destination}")
        {
        }
    }
}
