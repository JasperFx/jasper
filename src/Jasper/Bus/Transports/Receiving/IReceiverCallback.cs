using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Tcp;

namespace Jasper.Bus.Transports.Receiving
{
    public interface IReceiverCallback
    {
        Task<ReceivedStatus> Received(Uri uri, Envelope[] messages);
        Task Acknowledged(Envelope[] messages);
        Task NotAcknowledged(Envelope[] messages);
        Task Failed(Exception exception, Envelope[] messages);
    }
}
