using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Transports.Tcp;
using Jasper.Transports.Util;
using Jasper.Util;

namespace Jasper.Transports
{
    // This is only really used in the automated testing now
    // to test out the wire protocol. Otherwise, this has been superceded
    // by SocketListeningAgent
    public class ListeningAgent : IDisposable
    {
        private readonly IReceiverCallback _callback;
        private readonly CancellationToken _cancellationToken;
        private readonly TcpListener _listener;
        private readonly ActionBlock<Socket> _socketHandling;
        private readonly Uri _uri;
        private Task _receivingLoop;

        public ListeningAgent(IReceiverCallback callback, IPAddress ipaddr, int port, string protocol,
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
                WireProtocol.BeginReceiveResult result = await WireProtocol.BeginReceive(stream, _uri);
                
                if (result.Status == ReceivedStatus.Successful)
                {
                    try
                    {
                        if (result.Messages.Any() && result.Messages.First().IsPing())
                        {
                            await WireProtocol.EndReceive(stream, ReceivedStatus.Successful);
                            return;
                        }

                        await _callback.Received(_uri, result.Messages);
                    }
                    catch (Exception)
                    {
                        await WireProtocol.EndReceive(stream, ReceivedStatus.ProcessFailure);
                    }
                }

                await WireProtocol.EndReceive(stream, result.Status);
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
