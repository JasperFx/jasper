using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Tcp;
using Jasper.Bus.Transports.Util;

namespace Jasper.Bus.Transports.Sending
{
    public class SocketSenderProtocol : ISenderProtocol
    {
        public async Task SendBatch(ISenderCallback callback, OutgoingMessageBatch batch)
        {
            if (batch.Data.Length == 0) throw new Exception("No data to be sent");

            using (var client = new TcpClient())
            {
                var connection = connect(client, batch.Destination)
                    .TimeoutAfter(5000);

                await connection;

                if (connection.IsCompleted)
                {
                    using (var stream = client.GetStream())
                    {
                        var protocolTimeout = WireProtocol.Send(stream, batch, batch.Data, callback);
                        //var protocolTimeout = .TimeoutAfter(5000);
                        await protocolTimeout.ConfigureAwait(false);

                        if (!protocolTimeout.IsCompleted)
                        {
                            await callback.TimedOut(batch);
                        }

                        if (protocolTimeout.IsFaulted)
                        {
                            await callback.ProcessingFailure(batch, protocolTimeout.Exception);
                        }
                    }
                }
                else
                {
                    await callback.TimedOut(batch);
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
