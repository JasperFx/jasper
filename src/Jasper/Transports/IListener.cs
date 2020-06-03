using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jasper.Transports
{
    public interface IListener : IDisposable
    {
        Uri Address { get; }
        ListeningStatus Status { get; set; }
        IAsyncEnumerable<(Envelope Envelope, object AckObject)> Consume();
        Task Ack((Envelope Envelope, object AckObject) messageInfo);
        Task Nack((Envelope Envelope, object AckObject) messageInfo);
    }
}
