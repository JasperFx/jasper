using System;

namespace Jasper.Messaging.Transports.Receiving
{
    public interface IListener : IReceiverCallback, IDisposable
    {
        void Start();
        Uri Address { get; }
    }
}
