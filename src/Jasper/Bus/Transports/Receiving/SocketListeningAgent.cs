using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Bus.Transports.Tcp;
using Jasper.Util;

namespace Jasper.Bus.Transports.Receiving
{
    public class SocketListeningAgent : IListeningAgent
    {
        private readonly int _port;
        private readonly CancellationToken _cancellationToken;
        private TcpListener _listener;
        private ActionBlock<Socket> _socketHandling;
        private Task _receivingLoop;

        public SocketListeningAgent(int port, CancellationToken cancellationToken)
        {
            _port = port;
            _cancellationToken = cancellationToken;


            Address = $"tcp://{Environment.MachineName}:{port}/".ToUri();
        }

        public void Start(IReceiverCallback callback)
        {
            _listener = new TcpListener(new IPEndPoint(IPAddress.Loopback, _port));
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
