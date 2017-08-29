using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Jasper.Bus.Transports.Lightweight
{

    public class ListeningAgent : IDisposable
    {
        public int Port { get; }
        private readonly IReceiverCallback _callback;
        private readonly TcpListener _listener;
        private bool _isDisposed;
        private Task _receivingLoop;
        private ActionBlock<Socket> _socketHandling;

        public ListeningAgent(IReceiverCallback callback, int port)
        {
            Port = port;
            _callback = callback;
            _listener = new TcpListener(new IPEndPoint(IPAddress.Loopback, port));
            _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            _socketHandling = new ActionBlock<Socket>(s =>
            {
                var stream = new NetworkStream(s, true);
                return WireProtocol.Receive(stream, _callback);
            });

        }

        public void Start()
        {
            _receivingLoop = Task.Run(async () =>
            {
                _listener.Start();

                while (!_isDisposed)
                {
                    var socket = await _listener.AcceptSocketAsync();
                    _socketHandling.Post(socket);
                }
            });
        }

        public void Dispose()
        {
            _socketHandling.Complete();
            _listener.Stop();
            _isDisposed = true;
        }
    }
}
