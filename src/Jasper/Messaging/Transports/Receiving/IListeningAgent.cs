using System;

namespace Jasper.Messaging.Transports.Receiving
{
    public interface IListeningAgent : IDisposable
    {
        void Start(IReceiverCallback callback);
        Uri Address { get; }
        ListeningStatus Status { get; set; }
    }
}
