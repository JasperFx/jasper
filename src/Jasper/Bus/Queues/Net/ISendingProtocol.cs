using System;

namespace Jasper.Bus.Queues.Net
{
    public interface ISendingProtocol
    {
        IObservable<OutgoingMessage> Send(OutgoingMessageBatch batch);
    }
}