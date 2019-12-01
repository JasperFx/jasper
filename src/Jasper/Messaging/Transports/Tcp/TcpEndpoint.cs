using System;
using System.Linq;
using System.Net;
using Jasper.Configuration;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Sending;
using Jasper.Util;

namespace Jasper.Messaging.Transports.Tcp
{
    public class TcpEndpoint : Endpoint
    {
        public TcpEndpoint() : this("localhost", 2000)
        {
        }

        public TcpEndpoint(int port) : this ("localhost", port)
        {
        }

        public TcpEndpoint(string hostName, int port)
        {
            HostName = hostName;
            Port = port;

            Uri = $"tcp://{hostName}:{port}".ToUri();
        }

        public string HostName { get; set; } = "localhost";

        public int Port { get; private set; }

        public override void Parse(Uri uri)
        {
            if (uri.Scheme != "tcp")
            {
                throw new ArgumentOutOfRangeException(nameof(uri));
            }

            HostName = uri.Host;
            Port = uri.Port;

            Uri = $"tcp://{HostName}:{Port}".ToUri();

            if (uri.IsDurable())
            {
                IsDurable = true;
            }
        }

        protected internal override void StartListening(IMessagingRoot root, ITransportRuntime runtime)
        {
            if (!IsListener) return;

            var listener = createListener(root);
            runtime.AddListener(listener, this);
        }

        protected internal override ISendingAgent StartSending(IMessagingRoot root, ITransportRuntime runtime,
            Uri replyUri)
        {
            var sender = new BatchedSender(Uri, new SocketSenderProtocol(), root.Settings.Cancellation, root.TransportLogger);
            return runtime.AddSubscriber(replyUri, sender, Subscriptions.ToArray());
        }

        private IListener createListener(IMessagingRoot root)
        {
            // check the uri for an ip address to bind to
            var cancellation = root.Settings.Cancellation;

            var hostNameType = Uri.CheckHostName(HostName);

            if (hostNameType != UriHostNameType.IPv4 && hostNameType != UriHostNameType.IPv6)
                return HostName == "localhost"
                    ? new SocketListener(IPAddress.Loopback, Port, cancellation)
                    : new SocketListener(IPAddress.Any, Port, cancellation);

            var ipaddr = IPAddress.Parse(HostName);
            return new SocketListener(ipaddr, Port, cancellation);
        }
    }
}
