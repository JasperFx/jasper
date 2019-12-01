using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using Baseline;
using Jasper.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.Messaging.Transports.Tcp
{
    public class TcpTransport : TransportBase<TcpEndpoint>
    {
        private readonly LightweightCache<Uri, TcpEndpoint> _listeners =
            new LightweightCache<Uri, TcpEndpoint>(uri =>
            {
                var endpoint = new TcpEndpoint();
                endpoint.Parse(uri);

                return endpoint;
            });

        public TcpTransport() :
            base("tcp")
        {
        }

        protected override Uri canonicizeUri(Uri uri)
        {
            return new Uri($"tcp://{uri.Host}:{uri.Port}");
        }

        protected override IEnumerable<TcpEndpoint> endpoints()
        {
            return _listeners;
        }

        protected override TcpEndpoint findEndpointByUri(Uri uri)
        {
            return _listeners[uri];
        }

    }
}
