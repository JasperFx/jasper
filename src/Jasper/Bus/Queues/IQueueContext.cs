using System;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Queues
{
    public interface IQueueContext
    {
        void CommitChanges();
        void Send(OutgoingMessage message);
        void ReceiveLater(TimeSpan timeSpan);
        void ReceiveLater(DateTimeOffset time);
        void SuccessfullyReceived();
        void MoveTo(string queueName);
        void Enqueue(Envelope message);
    }
}
