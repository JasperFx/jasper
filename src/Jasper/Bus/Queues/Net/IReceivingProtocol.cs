using System;
using System.IO;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Queues.Net
{
    public interface IReceivingProtocol
    {
        IObservable<Envelope> ReceiveStream(IObservable<Stream> streams, string from);
    }
}
