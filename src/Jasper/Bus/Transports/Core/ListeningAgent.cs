using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Util;

namespace Jasper.Bus.Transports.Core
{

    public class ListeningAgent : IDisposable
    {
        public int Port { get; }
        private readonly IReceiverCallback _callback;
        private readonly CancellationToken _cancellationToken;
        private readonly TcpListener _listener;
        private Task _receivingLoop;
        private readonly ActionBlock<Socket> _socketHandling;
        private readonly Uri _uri;

        public ListeningAgent(IReceiverCallback callback, int port, string protocol, CancellationToken cancellationToken)
        {
            Port = port;
            _callback = callback;
            _cancellationToken = cancellationToken;
            _listener = new TcpListener(new IPEndPoint(IPAddress.Loopback, port));
            _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            _socketHandling = new ActionBlock<Socket>(s =>
            {
                var stream = new NetworkStream(s, true);
                return WireProtocol.Receive(stream, _callback, _uri);
            });

            _uri = $"{protocol}://{Environment.MachineName}:{port}/".ToUri();

        }

        public void Start()
        {
            _receivingLoop = Task.Run(async () =>
            {
                _listener.Start();

                while (!_cancellationToken.IsCancellationRequested)
                {
                    var socket = await _listener.AcceptSocketAsync();
                    _socketHandling.Post(socket);
                }
            }, _cancellationToken);
        }

        public void Dispose()
        {
            _socketHandling.Complete();
            _listener.Stop();
        }
    }
}
