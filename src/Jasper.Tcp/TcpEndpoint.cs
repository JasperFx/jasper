using System;
using System.Net;
using Jasper.Configuration;
using Jasper.Runtime;
using Jasper.Transports;
using Jasper.Transports.Sending;
using Jasper.Util;

namespace Jasper.Tcp
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

            Name = Uri.ToString();
        }

        protected override bool supportsMode(EndpointMode mode)
        {
            return mode != EndpointMode.Inline;
        }

        public override Uri Uri => ToUri(Port, HostName);

        public static Uri ToUri(int port, string hostName = "localhost")
        {
            return $"tcp://{hostName}:{port}".ToUri();
        }

        public override Uri ReplyUri()
        {
            var uri = ToUri(Port, HostName);
            if (Mode != EndpointMode.Durable)
            {
                return uri;
            }

            return $"{uri}durable".ToUri();
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
                Mode = EndpointMode.Durable;
            }
        }

        public override void StartListening(IMessagingRoot root, ITransportRuntime runtime)
        {
            if (!IsListener) return;

            var listener = createListener(root);
            runtime.AddListener(listener, this);
        }

        protected override ISender CreateSender(IMessagingRoot root)
        {
            return new BatchedSender(Uri, new SocketSenderProtocol(), root.Settings.Cancellation, root.TransportLogger);
        }

        private IListener createListener(IMessagingRoot root)
        {
            // check the uri for an ip address to bind to
            var cancellation = root.Settings.Cancellation;

            var hostNameType = Uri.CheckHostName(HostName);

            if (hostNameType != UriHostNameType.IPv4 && hostNameType != UriHostNameType.IPv6)
                return HostName == "localhost"
                    ? new SocketListener(root.TransportLogger,IPAddress.Loopback, Port, cancellation)
                    : new SocketListener(root.TransportLogger,IPAddress.Any, Port, cancellation);

            var ipaddr = IPAddress.Parse(HostName);
            return new SocketListener(root.TransportLogger, ipaddr, Port, cancellation);
        }
    }
}
