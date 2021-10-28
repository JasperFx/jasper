using System;
using System.Threading.Tasks;

namespace Jasper.Transports
{
    public interface IListener : IChannelCallback, IDisposable
    {
        Uri Address { get; }
        ListeningStatus Status { get; set; }
        void Start(IListeningWorkerQueue callback);

        Task<bool> TryRequeue(Envelope envelope);
    }

}
