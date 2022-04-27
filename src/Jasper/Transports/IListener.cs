using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jasper.Transports
{
    public interface IListener : IChannelCallback, IDisposable
    {
        Uri Address { get; }
        ListeningStatus Status { get; set; }
        void Start(IListeningWorkerQueue callback, CancellationToken cancellation);

        Task<bool> TryRequeue(Envelope envelope);
    }

}
