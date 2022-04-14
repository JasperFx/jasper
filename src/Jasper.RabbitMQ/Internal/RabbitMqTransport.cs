using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Runtime;
using Jasper.Transports;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public partial class RabbitMqTransport : TransportBase<RabbitMqEndpoint>, IRabbitMqTransport
    {
        public const string ProtocolName = "rabbitmq";

        private readonly LightweightCache<Uri, RabbitMqEndpoint> _endpoints;

        public RabbitMqTransport() : base(ProtocolName, "Rabbit MQ")
        {
            _endpoints =
                new LightweightCache<Uri, RabbitMqEndpoint>(uri =>
                {
                    var endpoint = new RabbitMqEndpoint();
                    endpoint.Parse(uri);

                    endpoint.Parent = this;

                    return endpoint;
                });

            Exchanges = new LightweightCache<string, RabbitMqExchange>(name => new RabbitMqExchange(name, this));
        }

        protected override IEnumerable<RabbitMqEndpoint> endpoints()
        {
            return _endpoints;
        }

        protected override RabbitMqEndpoint findEndpointByUri(Uri uri)
        {
            return _endpoints[uri];
        }

        public override void Initialize(IJasperRuntime root)
        {
            if (AutoProvision)
            {
                InitializeAllObjects();
            }

            if (AutoPurgeOnStartup)
            {
                PurgeAllQueues();
            }
        }

        public bool AutoProvision { get; set; } = false;
        public bool AutoPurgeOnStartup { get; set; } = false;

        public ConnectionFactory ConnectionFactory { get; } = new ConnectionFactory();

        public IList<AmqpTcpEndpoint> AmqpTcpEndpoints { get; } = new List<AmqpTcpEndpoint>();

        public LightweightCache<string, RabbitMqExchange> Exchanges { get; }

        public LightweightCache<string, RabbitMqQueue> Queues { get; }
            = new LightweightCache<string, RabbitMqQueue>(name => new RabbitMqQueue(name));

        public IList<Binding> Bindings { get; } = new List<Binding>();

        public void DeclareBinding(Binding binding)
        {
            binding.AssertValid();

            DeclareExchange(binding.ExchangeName);
            DeclareExchange(binding.QueueName);

            Bindings.Add(binding);
        }

        public void DeclareExchange(string exchangeName, Action<RabbitMqExchange> configure = null)
        {
            var exchange = Exchanges[exchangeName];
            configure?.Invoke(exchange);
        }

        public void DeclareQueue(string queueName, Action<RabbitMqQueue> configure = null)
        {
            var queue = Queues[queueName];
            configure?.Invoke(queue);
        }

        internal IConnection BuildConnection()
        {
            return AmqpTcpEndpoints.Any()
                ? ConnectionFactory.CreateConnection(AmqpTcpEndpoints)
                : ConnectionFactory.CreateConnection();
        }







        public RabbitMqEndpoint EndpointForQueue(string queueName)
        {
            // Yeah, it's super inefficient, but it only happens once or twice
            // when bootstrapping'
            var temp = new RabbitMqEndpoint {QueueName = queueName};
            return findEndpointByUri(temp.Uri);
        }

        public RabbitMqEndpoint EndpointFor(string routingKey, string exchangeName)
        {
            var temp = new RabbitMqEndpoint
            {
                RoutingKey = routingKey,
                ExchangeName = exchangeName
            };

            return findEndpointByUri(temp.Uri);
        }

        public RabbitMqEndpoint EndpointForExchange(string exchangeName)
        {
            var temp = new RabbitMqEndpoint{ExchangeName = exchangeName};
            return findEndpointByUri(temp.Uri);
        }


    }
}
