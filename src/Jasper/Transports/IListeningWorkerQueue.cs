using System;

namespace Jasper.Transports
{
    public interface IListeningWorkerQueue : IReceiverCallback, IDisposable
    {
        Uri Address { get; }
        ListeningStatus Status { get; set; }
    }
}
