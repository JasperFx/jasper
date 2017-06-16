using System;
using System.IO;

namespace Jasper.Bus.Queues.Net
{
    public interface IReceivingProtocol
    {
        IObservable<Message> ReceiveStream(IObservable<Stream> streams, string from);
    }
}
