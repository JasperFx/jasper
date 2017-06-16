using System;

namespace JasperBus.Queues.Net
{
    public interface ISendingProtocol
    {
        IObservable<OutgoingMessage> Send(OutgoingMessageBatch batch);
    }
}