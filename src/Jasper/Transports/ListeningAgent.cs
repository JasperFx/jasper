using System;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Runtime;
using Jasper.Runtime.WorkerQueues;

namespace Jasper.Transports;

public interface IListeningAgent
{
    Uri Uri { get; }
    ListeningStatus Status { get; }
    Endpoint Endpoint { get; }
    ValueTask StopAsync();
    ValueTask StartAsync();
}

internal class ListeningAgent : IAsyncDisposable, IDisposable, IListeningAgent
{
    private readonly IJasperRuntime _runtime;
    private IListener? _listener;
    private IReceiver? _receiver;

    public ListeningAgent(Endpoint endpoint, IJasperRuntime runtime)
    {
        Endpoint = endpoint;
        _runtime = runtime;
        Uri = endpoint.Uri;
    }

    public Endpoint Endpoint { get; }

    public Uri Uri { get; }

    public ListeningStatus Status { get; private set; } = ListeningStatus.Stopped;

    public async ValueTask StopAsync()
    {
        await DisposeAsync();
        Status = ListeningStatus.Stopped;
    }

    public ValueTask StartAsync()
    {
        _receiver = buildReceiver();

        _listener = Endpoint.BuildListener(_runtime, _receiver);

        Status = ListeningStatus.Accepting;

        return ValueTask.CompletedTask;
    }

    private IReceiver buildReceiver()
    {
        switch (Endpoint.Mode)
        {
            case EndpointMode.Durable:
                return new DurableReceiver(Endpoint, _runtime);

            case EndpointMode.Inline:
                return new InlineReceiver(_runtime);

            case EndpointMode.BufferedInMemory:
                return new BufferedReceiver(Endpoint, _runtime);

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
        if (_listener != null) await _listener.DisposeAsync();
        _receiver?.Dispose();

        _listener = null;
        _receiver = null;
    }
}
