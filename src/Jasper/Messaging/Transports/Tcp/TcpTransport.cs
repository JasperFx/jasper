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
        public TcpTransport(ITransportLogger logger, AdvancedSettings settings) :
            base("tcp", logger, settings)
        {
        }


        protected override ISender createSender(Uri uri, CancellationToken cancellation)
        {
            return new BatchedSender(uri, new SocketSenderProtocol(), cancellation, logger);
        }

        protected override ListenerSettings[] validateAndChooseReplyChannel(ListenerSettings[] incoming)
        {
            assertNoDuplicatePorts(incoming);

            ReplyUri = incoming.FirstOrDefault()?.Uri;

            return incoming;
        }


        protected override IListener buildListeningAgent(ListenerSettings listenerSettings,
            AdvancedSettings settings,
            HandlerGraph handlers)
        {
            // check the uri for an ip address to bind to
            var uri = listenerSettings.Uri;

            if (uri.HostNameType != UriHostNameType.IPv4 && uri.HostNameType != UriHostNameType.IPv6)
                return uri.Host == "localhost"
                    ? new SocketListener(IPAddress.Loopback, uri.Port, settings.Cancellation)
                    : new SocketListener(IPAddress.Any, uri.Port, settings.Cancellation);

            var ipaddr = IPAddress.Parse(uri.Host);
            return new SocketListener(ipaddr, uri.Port, settings.Cancellation);
        }

        private static void assertNoDuplicatePorts(ListenerSettings[] incoming)
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
