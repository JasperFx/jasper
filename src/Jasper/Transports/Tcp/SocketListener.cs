using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Transports.Util;
using Jasper.Util;
using Microsoft.Extensions.Logging;

namespace Jasper.Transports.Tcp;

public class SocketListener : IListener, IDisposable
{
    private CancellationToken _cancellationToken;
    private readonly IPAddress _ipaddr;
    private readonly ILogger _logger;
    private readonly int _port;
    private TcpListener? _listener;
    private Task? _receivingLoop;
    private ActionBlock<Socket>? _socketHandling;
    private IReceiver _receiver;
    private readonly CancellationToken _parentToken;
    private CancellationTokenSource _listenerCancellation;

    public SocketListener(IReceiver receiver, ILogger logger, IPAddress ipaddr, int port,
        CancellationToken cancellationToken)
    {
        _logger = logger;
        _port = port;
        _ipaddr = ipaddr;
        _parentToken = cancellationToken;

        Address = $"tcp://{ipaddr}:{port}/".ToUri();

        Start(receiver, cancellationToken);
    }

    [Obsolete]
    public void Start(IReceiver callback, CancellationToken cancellation)
    {

        _receiver = callback;

        startListening(callback);
    }

    private void startListening(IReceiver callback)
    {
        _listenerCancellation = new CancellationTokenSource();
        _cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(_parentToken, _listenerCancellation.Token).Token;

        _listener = new TcpListener(new IPEndPoint(_ipaddr, _port));
        _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        _socketHandling = new ActionBlock<Socket>(async s =>
        {
            await using var stream = new NetworkStream(s, true);
            await HandleStreamAsync(callback, stream);
        }, new ExecutionDataflowBlockOptions { CancellationToken = _cancellationToken });

        _receivingLoop = Task.Run(async () =>
        {
            _listener.Start();

            while (!_cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var socket = await _listener.AcceptSocketAsync(_cancellationToken);
                    await _socketHandling.SendAsync(socket, _cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }, _cancellationToken);
    }

    public async ValueTask StopAsync()
    {
        _listenerCancellation.Cancel();
        Status = ListeningStatus.Stopped;
        _listener?.Stop();
        _listener = null;

        if (_receivingLoop != null)
        {
            await _receivingLoop;
            _receivingLoop.Dispose();
            _receivingLoop = null;
        }

        if (_socketHandling != null)
        {
            _socketHandling.Complete();
            _socketHandling = null;
        }
    }

    public ValueTask RestartAsync()
    {
        startListening(_receiver);
        Status = ListeningStatus.Accepting;

        return ValueTask.CompletedTask;
    }

    public Task<bool> TryRequeueAsync(Envelope envelope)
    {
        return Task.FromResult(false);
    }

    public Uri Address { get; }

    public void Dispose()
    {
        _socketHandling?.Complete();
        _listener?.Stop();
        _listener?.Server.Dispose();
        _receivingLoop?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        _listenerCancellation.Cancel();
        Status = ListeningStatus.Stopped;
        _listener?.Stop();
        _listener = null;

        if (_receivingLoop != null)
        {
            await _receivingLoop;
            _receivingLoop.Dispose();
            _receivingLoop = null;
        }

        if (_socketHandling != null)
        {
            _socketHandling.Complete();
            _socketHandling = null;
        }
    }

    public ListeningStatus Status { get; internal set; } = ListeningStatus.Accepting;

    public ValueTask CompleteAsync(Envelope envelope)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask DeferAsync(Envelope envelope)
    {
        return ValueTask.CompletedTask;
    }

    public Task HandleStreamAsync(IReceiver? callback, Stream stream)
    {
        return Status == ListeningStatus.TooBusy
            ? stream.SendBufferAsync(WireProtocol.ProcessingFailureBuffer)
            : WireProtocol.ReceiveAsync(this, _logger, stream, callback);
    }
}
