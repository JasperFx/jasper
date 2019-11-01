using System;
using Jasper.Messaging.Transports;

namespace Jasper.Messaging.Runtime
{
    public interface IListener : IReceiverCallback, IDisposable
    {
        Uri Address { get; }
        ListeningStatus Status { get; set; }
        void Start();
    }
}
