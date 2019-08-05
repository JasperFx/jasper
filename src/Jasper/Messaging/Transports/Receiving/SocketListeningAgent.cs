using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Messaging.Transports.Tcp;
using Jasper.Messaging.Transports.Util;
using Jasper.Util;

namespace Jasper.Messaging.Transports.Receiving
{
    public class SocketListeningAgent : IListeningAgent
    {
        private readonly CancellationToken _cancellationToken;
        private readonly IPAddress _ipaddr;
        private readonly int _port;
        private TcpListener _listener;
        private Task _receivingLoop;
        private ActionBlock<Socket> _socketHandling;

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
                    await HandleStream(callback, stream);
                }
            }, new ExecutionDataflowBlockOptions{CancellationToken = _cancellationToken});

            _receivingLoop = Task.Run(async () =>
            {
                _listener.Start();

                while (!_cancellationToken.IsCancellationRequested)
                {
                    var socket = await _listener.AcceptSocketAsync();
                    await _socketHandling.SendAsync(socket, _cancellationToken);
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

        public ListeningStatus Status { get; set; } = ListeningStatus.Accepting;

        public Task HandleStream(IReceiverCallback callback, Stream stream)
        {
            if (Status == ListeningStatus.TooBusy) return stream.SendBuffer(WireProtocol.ProcessingFailureBuffer);

            return WireProtocol.Receive(stream, callback, Address);
        }
    }
}
