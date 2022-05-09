using System;
using System.Threading.Tasks;

namespace Jasper.Transports;

public interface IListeningWorkerQueue : IDisposable
{
    Task ReceivedAsync(Uri uri, Envelope[] messages);
    ValueTask ReceivedAsync(Uri uri, Envelope envelope);
}
