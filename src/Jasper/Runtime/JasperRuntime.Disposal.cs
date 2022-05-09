using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Baseline.ImTools;

namespace Jasper.Runtime;

public partial class JasperRuntime : IAsyncDisposable
{
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (!_hasStopped)
        {
            await StopAsync(CancellationToken.None);
        }

        foreach (var kv in _senders.Enumerate()) kv.Value.SafeDispose();

        foreach (var listener in _disposables) listener.SafeDispose();

        Advanced.Cancel();

        ScheduledJobs?.Dispose();
    }
}
