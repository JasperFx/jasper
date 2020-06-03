using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Logging;
using Jasper.Runtime;
using Jasper.Transports.Util;
using Jasper.Util;

namespace Jasper.Transports.Tcp
{
    public class SocketListener : IListener
    {
        private readonly CancellationToken _cancellationToken;
        private readonly IPAddress _ipaddr;
        private readonly ITransportLogger _logger;
        private readonly int _port;
        private TcpListener _listener;
        private Task _receivingLoop;
        private ActionBlock<Socket> _socketHandling;

        public SocketListener(ITransportLogger logger, IPAddress ipaddr, int port,
            CancellationToken cancellationToken)
        {
            _logger = logger;
            _port = port;
            _ipaddr = ipaddr;
            _cancellationToken = cancellationToken;

            Address = $"tcp://{ipaddr}:{port}/".ToUri();
        }

        public void Start(IListeningWorkerQueue callback)
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

        public void StartHandlingInline(IHandlerPipeline pipeline)
        {
            throw new NotImplementedException();
        }

        public Uri Address { get; }

        public void Dispose()
        {
            _socketHandling?.Complete();
            _listener?.Stop();
            _listener?.Server.Dispose();
        }

        public ListeningStatus Status { get; set; } = ListeningStatus.Accepting;

        public Task HandleStream(IListeningWorkerQueue callback, Stream stream)
        {
            if (Status == ListeningStatus.TooBusy) return stream.SendBuffer(WireProtocol.ProcessingFailureBuffer);

            return WireProtocol.Receive(_logger, stream, callback, Address);
        }
    }
}
