using System;
using System.Threading.Tasks;

namespace Jasper.Transports
{
    public interface IListeningWorkerQueue : IDisposable
    {
        Task Received(Uri uri, Envelope[] messages);
    }
}
