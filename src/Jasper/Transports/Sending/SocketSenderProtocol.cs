using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Jasper.Transports.Tcp;
using Jasper.Transports.Util;

namespace Jasper.Transports.Sending
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
                    using (var stream = client.GetStream())
                    {
                        var protocolTimeout = WireProtocol.Send(stream, batch, batch.Data);
                        //var protocolTimeout = .TimeoutAfter(5000);
                        WireProtocol.SendStatus result = await protocolTimeout.ConfigureAwait(false);

                        if (!protocolTimeout.IsCompleted) await callback.TimedOut(batch);

                        if (protocolTimeout.IsFaulted)
                            await callback.ProcessingFailure(batch, protocolTimeout.Exception);

                        switch (result)
                        {
                            case WireProtocol.SendStatus.Failure:
                                await callback.ProcessingFailure(batch);
                                break;
                            case WireProtocol.SendStatus.Success:
                                await callback.Successful(batch);
                                break;
                            case WireProtocol.SendStatus.SerializationFailure:
                                await callback.SerializationFailure(batch);
                                break;
                            case WireProtocol.SendStatus.QueueDoesNotExist:
                                await callback.QueueDoesNotExist(batch);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(WireProtocol.SendStatus));
                        }
                    }
                else
                    await callback.TimedOut(batch);
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
