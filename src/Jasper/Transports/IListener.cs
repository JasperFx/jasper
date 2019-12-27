using System;

namespace Jasper.Transports
{
    public interface IListener : IDisposable
    {
        Uri Address { get; }
        ListeningStatus Status { get; set; }
        void Start(IReceiverCallback callback);
    }
}
