using System;
using Jasper.Bus.Queues.Net;

namespace Jasper.Bus.Queues.New
{
    public interface ISenderCallback
    {
        void Successful(OutgoingMessageBatch outgoing);
        void TimedOut(OutgoingMessageBatch outgoing);
        void SerializationFailure(OutgoingMessageBatch outgoing);
        void QueueDoesNotExist(OutgoingMessageBatch outgoing);
        void ProcessingFailure(OutgoingMessageBatch outgoing);
        void ProcessingFailure(OutgoingMessageBatch outgoing, Exception exception);
    }
}