using System;
using System.Collections.Generic;
using System.Threading;
using Baseline;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Tcp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public class RabbitMqTransport : TransportBase<RabbitMqEndpoint>
    {
        public const string Protocol = "rabbitmq";

        private readonly LightweightCache<Uri, RabbitMqEndpoint> _listeners;

        public RabbitMqTransport() : base(Protocol)
        {
            _listeners =
                new LightweightCache<Uri, RabbitMqEndpoint>(uri =>
                {
                    var endpoint = new RabbitMqEndpoint();
                    endpoint.Parse(uri);

                    endpoint.Parent = this;

                    return endpoint;
                });
        }

        protected override IEnumerable<RabbitMqEndpoint> endpoints()
        {
            return _listeners;
        }

        protected override RabbitMqEndpoint findEndpointByUri(Uri uri)
        {
            return _listeners[uri];
        }

        public ConnectionFactory ConnectionFactory { get; } = new ConnectionFactory();

        public IList<AmqpTcpEndpoint> AmqpTcpEndpoints { get; } = new List<AmqpTcpEndpoint>();


    }
}
