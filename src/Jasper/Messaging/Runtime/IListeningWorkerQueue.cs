using System;
using Jasper.Messaging.Transports;

namespace Jasper.Messaging.Runtime
{
    public interface IListeningWorkerQueue : IReceiverCallback, IDisposable
    {
        Uri Address { get; }
        ListeningStatus Status { get; set; }
    }
}
