using System;
using Jasper.Bus.Transports.Tcp;

namespace Jasper.Bus.Transports.Sending
{
    // TODO -- make this be async all the way down
    public interface ISenderCallback
    {
        void Successful(OutgoingMessageBatch outgoing);
        void TimedOut(OutgoingMessageBatch outgoing);
        void SerializationFailure(OutgoingMessageBatch outgoing);
        void QueueDoesNotExist(OutgoingMessageBatch outgoing);
        void ProcessingFailure(OutgoingMessageBatch outgoing);
        void ProcessingFailure(OutgoingMessageBatch outgoing, Exception exception);
        void SenderIsLatched(OutgoingMessageBatch outgoing);

    }
}
