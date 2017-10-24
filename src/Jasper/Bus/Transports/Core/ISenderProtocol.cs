using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Jasper.Bus.Transports.Util;

namespace Jasper.Bus.Transports.Core
{
    public interface ISenderProtocol
    {
        Task SendBatch(ISenderCallback callback, OutgoingMessageBatch batch);
    }

    public class SocketSenderProtocol : ISenderProtocol
    {
        public async Task SendBatch(ISenderCallback callback, OutgoingMessageBatch batch)
        {
            using (var client = new TcpClient())
            {
                var connection = connect(client, batch.Destination)
                    .TimeoutAfter(5000);

                await connection;

                if (connection.IsCompleted)
                {
                    using (var stream = client.GetStream())
                    {
                        var protocolTimeout = WireProtocol.Send(stream, batch, callback).TimeoutAfter(5000);
                        await protocolTimeout;

                        if (!protocolTimeout.IsCompleted)
                        {
                            callback.TimedOut(batch);
                        }
                    }
                }
                else
                {
                    callback.TimedOut(batch);
                }
            }
        }

        private Task connect(TcpClient client, Uri destination)
        {
            return string.Equals(Dns.GetHostName(), destination.Host, StringComparison.OrdinalIgnoreCase)
                   || destination.Host == "localhost"
                   || destination.Host == "127.0.0.1"

                ? client.ConnectAsync(IPAddress.Loopback, destination.Port)
                : client.ConnectAsync(destination.Host, destination.Port);
        }
    }
}
