using System;
using System.Linq;
using System.Net;
using System.Threading;
using Jasper.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.Messaging.Transports.Tcp
{
    public class TcpTransport : TransportBase
    {
        public TcpTransport() :
            base("tcp")
        {
        }

        protected override IListener createListener(Endpoint settings, IMessagingRoot root)
        {
            // check the uri for an ip address to bind to
            var uri = settings.Uri;
            var cancellation = root.Settings.Cancellation;

            if (uri.HostNameType != UriHostNameType.IPv4 && uri.HostNameType != UriHostNameType.IPv6)
                return uri.Host == "localhost"
                    ? new SocketListener(IPAddress.Loopback, uri.Port, cancellation)
                    : new SocketListener(IPAddress.Any, uri.Port, cancellation);

            var ipaddr = IPAddress.Parse(uri.Host);
            return new SocketListener(ipaddr, uri.Port, cancellation);
        }

        public override ISender CreateSender(Uri uri, CancellationToken cancellation, IMessagingRoot root)
        {
            return new BatchedSender(uri, new SocketSenderProtocol(), cancellation, root.TransportLogger);
        }


        // TODO -- bring this back!
        private static void assertNoDuplicatePorts(Endpoint[] incoming)
        {
            var duplicatePorts = incoming.GroupBy(x => x.Uri.Port).Where(x => x.Count() > 1).ToArray();
            if (duplicatePorts.Any())
            {
                var portString = string.Join(", ", duplicatePorts.Select(x => x.ToString()));
                throw new Exception("Multiple TCP listeners configured for ports " + portString);
            }
        }


    }
}
