using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Util;

namespace Jasper.Transports.Tcp
{
    public class SocketListener : IListener
    {
        private readonly CancellationToken _cancellationToken;
        private readonly IPAddress _ipaddr;
        private readonly int _port;
        private TcpListener _tcpListener;

        public SocketListener(IPAddress ipaddr, int port, CancellationToken cancellationToken)
        {
            _port = port;
            _ipaddr = ipaddr;
            _cancellationToken = cancellationToken;

            Address = $"tcp://{ipaddr}:{port}/".ToUri();
        }

        public async IAsyncEnumerable<Envelope> Consume()
        {
            _tcpListener = new TcpListener(new IPEndPoint(_ipaddr, _port));
            _tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _tcpListener.Start();

            while (!_cancellationToken.IsCancellationRequested)
            {
                Socket socket = await _tcpListener.AcceptSocketAsync();

                await using var stream = new NetworkStream(socket, true);
                
                if (Status == ListeningStatus.TooBusy)
                {
                    await WireProtocol.EndReceive(stream, ReceivedStatus.ProcessFailure);
                    continue;
                }

                WireProtocol.BeginReceiveResult result = await WireProtocol.BeginReceive(stream, Address);

                if (result.Status != ReceivedStatus.Successful) continue;

                foreach (Envelope resultMessage in result.Messages)
                {
                    yield return resultMessage;
                }

                await WireProtocol.EndReceive(stream, ReceivedStatus.Successful);
            }
        }

        public Task<bool> Acknowledge(Envelope envelope)
        {
            throw new NotImplementedException();
        }

        public Uri Address { get; }

        public void Dispose()
        {
            _tcpListener?.Stop();
            _tcpListener?.Server.Dispose();
        }

        public ListeningStatus Status { get; set; } = ListeningStatus.Accepting;
    }
}
