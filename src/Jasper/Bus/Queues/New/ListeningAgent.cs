using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Jasper.Bus.Queues.New
{

    public class ListeningAgent : IDisposable
    {
        public int Port { get; }
        private readonly IReceiverCallback _callback;
        private readonly TcpListener _listener;
        private bool _isDisposed;
        private Task _receivingLoop;

        public ListeningAgent(IReceiverCallback callback, int port)
        {
            Port = port;
            _callback = callback;
            _listener = new TcpListener(new IPEndPoint(IPAddress.Loopback, port));
            _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        }

        public void Start()
        {
            _receivingLoop = Task.Run(async () =>
            {
                _listener.Start();

                while (!_isDisposed)
                {
                    // TODO -- might parallelize the receiving so that it passes off
                    // sockets as fast as possible?
                    var socket = await _listener.AcceptSocketAsync();
                    var stream = new NetworkStream(socket, true);
                    await WireProtocol.Receive(stream, _callback);
                }
            });
        }

        public void Dispose()
        {
            _listener.Stop();
            _isDisposed = true;
        }
    }
}
