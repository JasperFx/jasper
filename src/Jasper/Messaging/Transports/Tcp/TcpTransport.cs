using System;
using System.Linq;
using System.Net;
using System.Threading;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Transports.Receiving;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.Messaging.Transports.Tcp
{
    public class TcpTransport : TransportBase
    {
        public TcpTransport(ITransportLogger logger, JasperOptions options) :
            base("tcp", logger, options)
        {
        }


        protected override ISender createSender(Uri uri, CancellationToken cancellation)
        {
            return new BatchedSender(uri, new SocketSenderProtocol(), cancellation, logger);
        }

        protected override Uri[] validateAndChooseReplyChannel(Uri[] incoming)
        {
            assertNoDuplicatePorts(incoming);

            ReplyUri = incoming.FirstOrDefault();

            return incoming;
        }


        protected override IListeningAgent buildListeningAgent(Uri uri, JasperOptions settings, HandlerGraph handlers)
        {
            // check the uri for an ip address to bind to
            if (uri.HostNameType != UriHostNameType.IPv4 && uri.HostNameType != UriHostNameType.IPv6)
                return uri.Host == "localhost"
                    ? new SocketListeningAgent(IPAddress.Loopback, uri.Port, settings.Cancellation)
                    : new SocketListeningAgent(IPAddress.Any, uri.Port, settings.Cancellation);

            var ipaddr = IPAddress.Parse(uri.Host);
            return new SocketListeningAgent(ipaddr, uri.Port, settings.Cancellation);
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
