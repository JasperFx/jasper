using System;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Queues.Net
{
    public interface ISendingProtocol
    {
        IObservable<Envelope> Send(OutgoingMessageBatch batch);
    }
}
