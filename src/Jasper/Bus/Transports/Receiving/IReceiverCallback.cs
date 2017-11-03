using System;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Tcp;

namespace Jasper.Bus.Transports.Receiving
{
    public interface IReceiverCallback
    {
        ReceivedStatus Received(Uri uri, Envelope[] messages);
        void Acknowledged(Envelope[] messages);
        void NotAcknowledged(Envelope[] messages);
        void Failed(Exception exception, Envelope[] messages);
    }
}
