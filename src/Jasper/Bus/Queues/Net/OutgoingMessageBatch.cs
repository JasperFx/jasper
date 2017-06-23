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
        // TODO -- get rid of the ctor arg for the TcpClient
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


        public void Dispose()
        {
            using (Client)
            {
            }
            Client = null;
        }

        public override string ToString()
        {
            return $"Outgoing batch to {Destination} with {Messages.Count} messages";
        }
    }
}
