using System;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Transports.Core
{
    public interface IReceiverCallback
    {
        ReceivedStatus Received(Uri uri, Envelope[] messages);
        void Acknowledged(Envelope[] messages);
        void NotAcknowledged(Envelope[] messages);
        void Failed(Exception exception, Envelope[] messages);
    }
}
