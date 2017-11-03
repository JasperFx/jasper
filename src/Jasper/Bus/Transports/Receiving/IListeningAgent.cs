using System;

namespace Jasper.Bus.Transports.Receiving
{
    public interface IListeningAgent : IDisposable
    {
        void Start(IReceiverCallback callback);
        Uri Address { get; }
    }
}
