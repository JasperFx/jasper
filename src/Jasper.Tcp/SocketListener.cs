using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Transports;
using Jasper.Transports.Util;
using Jasper.Util;
using Microsoft.Extensions.Logging;

namespace Jasper.Tcp;

public class SocketListener : IListener, IDisposable
{
    private readonly CancellationToken _cancellationToken;
    private readonly IPAddress _ipaddr;
    private readonly ILogger _logger;
    private readonly int _port;
    private TcpListener? _listener;
    private Task? _receivingLoop;
    private ActionBlock<Socket>? _socketHandling;

    public SocketListener(ILogger logger, IPAddress ipaddr, int port,
        CancellationToken cancellationToken)
    {
        _logger = logger;
        _port = port;
        _ipaddr = ipaddr;
        _cancellationToken = cancellationToken;

        Address = $"tcp://{ipaddr}:{port}/".ToUri();
    }

    public void Start(IListeningWorkerQueue callback, CancellationToken cancellation)
    {
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
                var socket = await _listener.AcceptSocketAsync(_cancellationToken);
                await _socketHandling.SendAsync(socket, _cancellationToken);
            }
        }, _cancellationToken);
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

    public ListeningStatus Status { get; set; } = ListeningStatus.Accepting;

    public ValueTask CompleteAsync(Envelope envelope)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask DeferAsync(Envelope envelope)
    {
        return ValueTask.CompletedTask;
    }

    public Task HandleStreamAsync(IListeningWorkerQueue? callback, Stream stream)
    {
        return Status == ListeningStatus.TooBusy
            ? stream.SendBufferAsync(WireProtocol.ProcessingFailureBuffer)
            : WireProtocol.ReceiveAsync(_logger, stream, callback, Address);
    }
}
