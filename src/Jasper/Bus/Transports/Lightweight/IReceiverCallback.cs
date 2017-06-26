using System;
using Jasper.Bus.Queues;

namespace Jasper.Bus.Transports.Lightweight
{
    public interface IReceiverCallback
    {
        ReceivedStatus Received(Message[] messages);
        void Acknowledged(Message[] messages);
        void NotAcknowledged(Message[] messages);
        void Failed(Exception exception, Message[] messages);
    }
}