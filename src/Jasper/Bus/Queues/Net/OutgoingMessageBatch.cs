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
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Queues.Net
{
    public class OutgoingMessageBatch : IDisposable
    {
        // TODO -- get rid of the ctor arg for the TcpClient
        public OutgoingMessageBatch(Uri destination, IEnumerable<Envelope> messages)
        {
            Destination = destination;
            var messagesList = new List<Envelope>();
            messagesList.AddRange(messages);
            Messages = messagesList;
            Client = new TcpClient();
        }

        public Uri Destination { get; set; }

        public Stream Stream => Client.GetStream();

        public TcpClient Client { get; set; }
        public IList<Envelope> Messages { get; }

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
