using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Transports;
using Jasper.Transports.Tcp;
using Jasper.Util;
using Microsoft.Extensions.Logging.Abstractions;

namespace Jasper.Testing.Transports.Tcp
{
    // This is only really used in the automated testing now
    // to test out the wire protocol. Otherwise, this has been superceded
    // by SocketListeningAgent
    public class ListeningAgent : IDisposable
    {
        private readonly IListeningWorkerQueue _callback;
        private readonly CancellationToken _cancellationToken;
        private readonly TcpListener _listener;
        private readonly ActionBlock<Socket> _socketHandling;
        private readonly Uri _uri;
        private Task _receivingLoop;

        public ListeningAgent(IListeningWorkerQueue callback, IPAddress ipaddr, int port, string protocol,
            CancellationToken cancellationToken)
        {
            Port = port;
            _callback = callback;
            _cancellationToken = cancellationToken;

            _listener = new TcpListener(new IPEndPoint(ipaddr, port));
            _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            _socketHandling = new ActionBlock<Socket>(async s =>
            {
                await using var stream = new NetworkStream(s, true);
                await WireProtocol.ReceiveAsync(NullLogger.Instance, stream, _callback, _uri);
            }, new ExecutionDataflowBlockOptions{CancellationToken = _cancellationToken});

            _uri = $"{protocol}://{ipaddr}:{port}/".ToUri();
        }

        public int Port { get; }

        public void Dispose()
        {
            _socketHandling.Complete();
            _listener.Stop();
            _listener.Server.Dispose();
        }

        public void Start()
        {
            _listener.Start();

            _receivingLoop = Task.Run(async () =>
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    var socket = await _listener.AcceptSocketAsync();
                    await _socketHandling.SendAsync(socket, _cancellationToken);
                }
            }, _cancellationToken);
        }
    }
}
