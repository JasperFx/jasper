using System;
using System.Threading.Tasks;
using Jasper.Messaging.Transports.Tcp;

namespace Jasper.Messaging.Runtime
{
    public interface IReceiverCallback
    {
        Task<ReceivedStatus> Received(Uri uri, Envelope[] messages);
        Task Acknowledged(Envelope[] messages);
        Task NotAcknowledged(Envelope[] messages);
        Task Failed(Exception exception, Envelope[] messages);
    }
}
