using System;

namespace Jasper.Messaging.Transports.Receiving
{
    public interface IListener : IReceiverCallback, IDisposable
    {
        Uri Address { get; }
        ListeningStatus Status { get; set; }
        void Start();
    }
}
