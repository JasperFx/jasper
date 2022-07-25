using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jasper.Transports;

public interface IListener : IChannelCallback, IAsyncDisposable
{
    Uri Address { get; }
    ListeningStatus Status { get; }
    void Start(IReceiver callback, CancellationToken cancellation);

    [Obsolete]
    ValueTask StopAsync();
    [Obsolete] ValueTask RestartAsync();
}
