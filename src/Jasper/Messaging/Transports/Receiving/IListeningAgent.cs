using System;

namespace Jasper.Messaging.Transports.Receiving
{
    public interface IListeningAgent : IDisposable
    {
        Uri Address { get; }
        ListeningStatus Status { get; set; }
        void Start(IReceiverCallback callback);
    }
}
