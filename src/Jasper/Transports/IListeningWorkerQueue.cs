using System;
using System.Threading.Tasks;
using Jasper.Transports.Tcp;

namespace Jasper.Transports
{
    public interface IListeningWorkerQueue : IDisposable
    {
        Task<ReceivedStatus> Received(Uri uri, Envelope[] messages);
    }
}
