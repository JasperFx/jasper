using System;
using Jasper.Bus.Queues;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Transports.Lightweight
{
    public interface IReceiverCallback
    {
        ReceivedStatus Received(Envelope[] messages);
        void Acknowledged(Envelope[] messages);
        void NotAcknowledged(Envelope[] messages);
        void Failed(Exception exception, Envelope[] messages);
    }
}
