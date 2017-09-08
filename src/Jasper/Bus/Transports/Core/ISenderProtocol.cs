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

    public class SocketSenderProtocal : ISenderProtocol
    {
        public async Task SendBatch(ISenderCallback callback, OutgoingMessageBatch batch)
        {
            using (var client = new TcpClient())
            {
                await connect(client, batch.Destination)
                    .TimeoutAfter(5000);

                using (var stream = client.GetStream())
                {
                    await WireProtocol.Send(stream, batch, callback).TimeoutAfter(5000);
                }
            }
        }

        private Task connect(TcpClient client, Uri destination)
        {
            return Dns.GetHostName() == destination.Host
                   || destination.Host == "localhost"
                   || destination.Host == "127.0.0.1"

                ? client.ConnectAsync(IPAddress.Loopback, destination.Port)
                : client.ConnectAsync(destination.Host, destination.Port);
        }
    }
}
