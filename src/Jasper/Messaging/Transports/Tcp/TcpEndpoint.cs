using System;
using System.Net;
using Jasper.Configuration;
using Jasper.Messaging.Runtime;
using Jasper.Util;

namespace Jasper.Messaging.Transports.Tcp
{
    public class TcpEndpoint : Endpoint
    {
        public TcpEndpoint()
        {
        }

        public TcpEndpoint(int port)
        {
            Port = port;

        }

        public TcpEndpoint(string hostName, int port)
        {
            HostName = hostName;
            Port = port;
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
