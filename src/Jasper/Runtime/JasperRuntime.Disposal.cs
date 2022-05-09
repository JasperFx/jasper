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

        foreach (var kv in _senders.Enumerate())
        {
            var sender = kv.Value;
            if (sender is IAsyncDisposable ad)
            {
                await ad.DisposeAsync();
            }
            else if (sender is IDisposable d)
            {
                d.Dispose();
            }
        }

        foreach (var listener in _disposables)
        {
            if (listener is IAsyncDisposable ad)
            {
                await ad.DisposeAsync();
            }
            else if (listener is IDisposable d)
            {
                d.SafeDispose();
            }
        }

        Advanced.Cancel();

        ScheduledJobs?.Dispose();
    }
}
