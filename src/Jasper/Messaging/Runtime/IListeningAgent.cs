using System;
using Jasper.Messaging.Transports;

namespace Jasper.Messaging.Runtime
{
    public interface IListeningAgent : IDisposable
    {
        Uri Address { get; }
        ListeningStatus Status { get; set; }
        void Start(IReceiverCallback callback);
    }
}
