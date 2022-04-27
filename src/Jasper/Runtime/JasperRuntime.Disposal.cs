using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jasper.Runtime;

public partial class JasperRuntime : IAsyncDisposable
{
    void IDisposable.Dispose()
    {

    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (_hasStopped)
        {
            await StopAsync(CancellationToken.None);
        }

        Advanced.Cancel();

        Runtime.Dispose();

        ScheduledJobs?.Dispose();
    }
}
