using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Jasper.Bus.Queues.Logging;
using Jasper.Bus.Queues.Net.Protocol;
using Jasper.Bus.Queues.Net.Protocol.V1;
using Jasper.Bus.Queues.Serialization;
using Jasper.Bus.Queues.Storage;

namespace Jasper.Bus.Queues.Net
{
    public class OutgoingMessageBatch : IDisposable
    {
        public OutgoingMessageBatch(Uri destination, IEnumerable<OutgoingMessage> messages, TcpClient client)
        {
            Destination = destination;
            var messagesList = new List<OutgoingMessage>();
            messagesList.AddRange(messages);
            Messages = messagesList;
            Client = client;
        }

        public Uri Destination { get; set; }
        public Stream Stream => Client.GetStream();
        public TcpClient Client { get; set; }
        public IList<OutgoingMessage> Messages { get; }

        public Task ConnectAsync()
        {
            if(Dns.GetHostName() == Destination.Host)
            {
                return Client.ConnectAsync(IPAddress.Loopback, Destination.Port);
            }

            return Client.ConnectAsync(Destination.Host, Destination.Port);
        }

        public async Task<bool> WriteMessages(IMessageStore store, ILogger logger)
        {
            var messageBytes = Messages.Serialize();
            var lengthBytes = BitConverter.GetBytes(messageBytes.Length);


            await Stream.WriteAsync(lengthBytes, 0, lengthBytes.Length);

            await Stream.WriteAsync(messageBytes, 0, messageBytes.Length);

            var bytes = await Stream.ReadBytesAsync(Constants.ReceivedBuffer.Length).ConfigureAwait(false);
            if (bytes.SequenceEqual(Constants.ReceivedBuffer))
            {
                logger.DebugFormat("Read received bytes from {0}", Destination);
                store.SuccessfullySent(Messages.ToArray());
                logger.DebugFormat("Wrote acknowledgement to {0}", Destination);

                await Stream.WriteAsync(Constants.AcknowledgedBuffer, 0, Constants.AcknowledgedBuffer.Length);


                return true;
            }
            if (bytes.SequenceEqual(Constants.SerializationFailureBuffer))
            {
                throw new IOException("Failed to send messages, received serialization failed message.");
            }
            if (bytes.SequenceEqual(Constants.QueueDoesNotExistBuffer))
            {
                throw new QueueDoesNotExistException();
            }

            return false;
        }

        public void Dispose()
        {
            using (Client)
            {
            }
            Client = null;
        }
    }
}
