using System;

namespace Jasper.Bus.Queues.New
{
    public interface IReceiverCallback
    {
        ReceivedStatus Received(Message[] messages);
        void Acknowledged(Message[] messages);
        void NotAcknowledged(Message[] messages);
        void Failed(Exception exception, Message[] messages);
    }
}