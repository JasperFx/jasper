using System;
using System.IO;

namespace JasperBus.Queues.Net
{
    public interface IReceivingProtocol
    {
        IObservable<Message> ReceiveStream(IObservable<Stream> streams, string from);
    }
}
