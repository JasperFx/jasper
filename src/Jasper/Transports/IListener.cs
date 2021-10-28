using System;

namespace Jasper.Transports
{
    public interface IListener : IChannelCallback, IDisposable
    {
        Uri Address { get; }
        ListeningStatus Status { get; set; }
        void Start(IListeningWorkerQueue callback);
    }
}
