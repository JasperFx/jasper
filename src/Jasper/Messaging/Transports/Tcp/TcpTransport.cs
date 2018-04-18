using System;
using System.Linq;
using System.Net;
using System.Threading;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.Transports.Receiving;
using Jasper.Messaging.Transports.Sending;
using Jasper.Util;

namespace Jasper.Messaging.Transports.Tcp
{
    public class TcpTransport : TransportBase
    {
        public TcpTransport(IDurableMessagingFactory factory, ITransportLogger logger, MessagingSettings settings) : base("tcp", factory, logger, settings)
        {
        }


        protected override ISender createSender(Uri uri, CancellationToken cancellation)
        {
            return new BatchedSender(uri, new SocketSenderProtocol(), cancellation, logger);
        }

        protected override Uri[] validateAndChooseReplyChannel(Uri[] incoming)
        {
            assertNoDuplicatePorts(incoming);

            if (incoming.Any())
            {
                var uri = incoming.First();
                LocalReplyUri = uri.ToMachineUri();
            }

            return incoming;
        }


        protected override IListeningAgent buildListeningAgent(Uri uri, MessagingSettings settings)
        {
            // check the uri for an ip address to bind to
            if (uri.HostNameType == UriHostNameType.IPv4 || uri.HostNameType == UriHostNameType.IPv6)
            {
                IPAddress ipaddr = IPAddress.Parse(uri.Host);
                return new SocketListeningAgent(ipaddr, uri.Port, settings.Cancellation);
            }

            return uri.Host == "localhost"
                ? new SocketListeningAgent(IPAddress.Loopback, uri.Port, settings.Cancellation)
                : new SocketListeningAgent(IPAddress.Any, uri.Port, settings.Cancellation);
        }

        private static void assertNoDuplicatePorts(Uri[] incoming)
        {
            var duplicatePorts = incoming.GroupBy(x => x.Port).Where(x => x.Count() > 1).ToArray();
            if (duplicatePorts.Any())
            {
                var portString = string.Join(", ", duplicatePorts.Select(x => x.ToString()));
                throw new Exception("Multiple TCP listeners configured for ports " + portString);
            }
        }
    }
}
