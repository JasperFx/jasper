using System;
using System.Threading.Tasks;
using Jasper.Messaging.Transports.Tcp;

namespace Jasper.Messaging.Transports.Sending
{
    public interface ISenderCallback
    {
        Task Successful(OutgoingMessageBatch outgoing);
        Task TimedOut(OutgoingMessageBatch outgoing);
        Task SerializationFailure(OutgoingMessageBatch outgoing);
        Task QueueDoesNotExist(OutgoingMessageBatch outgoing);
        Task ProcessingFailure(OutgoingMessageBatch outgoing);
        Task ProcessingFailure(OutgoingMessageBatch outgoing, Exception exception);
        Task SenderIsLatched(OutgoingMessageBatch outgoing);

    }
}
