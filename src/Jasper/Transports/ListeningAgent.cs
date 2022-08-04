using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Configuration;
using Jasper.Runtime;
using Jasper.Runtime.WorkerQueues;
using Microsoft.Extensions.Logging;

namespace Jasper.Transports;

public interface IListeningAgent
{
    Uri Uri { get; }
    ListeningStatus Status { get; }
    Endpoint Endpoint { get; }
    ValueTask StopAsync();
    ValueTask StartAsync();
    ValueTask PauseAsync(TimeSpan pauseTime);
}

internal class ListeningAgent : IAsyncDisposable, IDisposable, IListeningAgent
{
    private readonly JasperRuntime _runtime;
    private IListener? _listener;
    private IReceiver? _receiver;
    private Restarter? _restarter;
    private readonly ILogger _logger;

    public ListeningAgent(Endpoint endpoint, JasperRuntime runtime)
    {
        Endpoint = endpoint;
        _runtime = runtime;
        Uri = endpoint.Uri;
        _logger = runtime.Logger;
    }

    public Endpoint Endpoint { get; }

    public Uri Uri { get; }

    public ListeningStatus Status { get; private set; } = ListeningStatus.Stopped;

    public async ValueTask StopAsync()
    {
        if (Status == ListeningStatus.Stopped) return;

        // TODO -- needs to drain outstanding messages in the listener
        await DisposeAsync();
        Status = ListeningStatus.Stopped;

        _logger.LogInformation("Stopped message listener at {Uri}", Uri);
    }

    public ValueTask StartAsync()
    {
        if (Status == ListeningStatus.Accepting) return ValueTask.CompletedTask;

        _receiver = buildReceiver();

        _listener = Endpoint.BuildListener(_runtime, _receiver);

        Status = ListeningStatus.Accepting;

        _logger.LogInformation("Started message listening at {Uri}", Uri);

        return ValueTask.CompletedTask;
    }

    public async ValueTask PauseAsync(TimeSpan pauseTime)
    {
        await StopAsync();

        _logger.LogInformation("Pausing message listening at {Uri}", Uri);

        _restarter = new Restarter(this, pauseTime);

    }

    private IReceiver buildReceiver()
    {
        switch (Endpoint.Mode)
        {
            case EndpointMode.Durable:
                return new DurableReceiver(Endpoint, _runtime, _runtime.Pipeline);

            case EndpointMode.Inline:
                return new InlineReceiver(_runtime, _runtime.Pipeline);

            case EndpointMode.BufferedInMemory:
                return new BufferedReceiver(Endpoint, _runtime, _runtime.Pipeline);

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void Dispose()
    {
        _receiver?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        _restarter?.SafeDispose();

        if (_listener != null) await _listener.DisposeAsync();
        _receiver?.Dispose();

        _listener = null;
        _receiver = null;
    }

    internal class Restarter : IDisposable
    {
        private readonly CancellationTokenSource _cancellation;
        private readonly Task<Task> _task;

        public Restarter(ListeningAgent parent, TimeSpan timeSpan)
        {
            _cancellation = new CancellationTokenSource();
            _task = Task.Delay(timeSpan, _cancellation.Token)
                .ContinueWith(async t =>
                {
                    if (_cancellation.IsCancellationRequested) return;
                    await parent.StartAsync();
                }, TaskScheduler.Default);
        }


        public void Dispose()
        {
            _cancellation.Cancel();
            _task.SafeDispose();
        }
    }
}
