using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Transports.Core
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
