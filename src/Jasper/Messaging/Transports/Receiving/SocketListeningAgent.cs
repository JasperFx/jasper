using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Messaging.Transports.Tcp;
using Jasper.Util;

namespace Jasper.Messaging.Transports.Receiving
{
    public class SocketListeningAgent : IListeningAgent
    {
        private readonly IPAddress _ipaddr;
        private readonly int _port;
        private readonly CancellationToken _cancellationToken;
        private TcpListener _listener;
        private ActionBlock<Socket> _socketHandling;
        private Task _receivingLoop;

        public SocketListeningAgent(IPAddress ipaddr, int port, CancellationToken cancellationToken)
        {
            _port = port;
            _ipaddr = ipaddr;
            _cancellationToken = cancellationToken;

            Address = $"tcp://{ipaddr}:{port}/".ToUri();
        }

        public void Start(IReceiverCallback callback)
        {
            _listener = new TcpListener(new IPEndPoint(_ipaddr, _port));
            _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            _socketHandling = new ActionBlock<Socket>(async s =>
            {
                using (var stream = new NetworkStream(s, true))
                {
                    await WireProtocol.Receive(stream, callback, Address);
                }
            });

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

        public Uri Address { get; }

        public void Dispose()
        {
            _socketHandling?.Complete();
            _listener?.Stop();
            _listener?.Server.Dispose();
        }
    }
}
