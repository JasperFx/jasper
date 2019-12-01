using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using Jasper.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.Messaging.Transports.Tcp
{
    public class TcpTransport : TransportBase<TcpEndpoint>
    {
        public TcpTransport() :
            base("tcp")
        {
        }

        protected override IListener createListener(TcpEndpoint endpoint, IMessagingRoot root)
        {
            // check the uri for an ip address to bind to
            var cancellation = root.Settings.Cancellation;

            var hostNameType = System.Uri.CheckHostName(endpoint.HostName);

            if (hostNameType != UriHostNameType.IPv4 && hostNameType != UriHostNameType.IPv6)
                return endpoint.HostName == "localhost"
                    ? new SocketListener(IPAddress.Loopback, endpoint.Port, cancellation)
                    : new SocketListener(IPAddress.Any, endpoint.Port, cancellation);

            var ipaddr = IPAddress.Parse(endpoint.HostName);
            return new SocketListener(ipaddr, endpoint.Port, cancellation);
        }

        public override ISender CreateSender(Uri uri, CancellationToken cancellation, IMessagingRoot root)
        {
            return new BatchedSender(uri, new SocketSenderProtocol(), cancellation, root.TransportLogger);
        }



    }
}
