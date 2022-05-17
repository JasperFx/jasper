using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Runtime;
using Jasper.Transports;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public partial class RabbitMqTransport : TransportBase<RabbitMqEndpoint>, IDisposable
    {
        public const string ProtocolName = "rabbitmq";

        private readonly LightweightCache<Uri, RabbitMqEndpoint> _endpoints;
        private IConnection? _listenerConnection;
        private IConnection? _sendingConnection;

        public RabbitMqTransport() : base(ProtocolName, "Rabbit MQ")
        {
            ConnectionFactory.AutomaticRecoveryEnabled = true;

            _endpoints =
                new LightweightCache<Uri, RabbitMqEndpoint>(uri =>
                {
                    var endpoint = new RabbitMqEndpoint(this);
                    endpoint.Parse(uri);

                    return endpoint;
                });

            Exchanges = new LightweightCache<string, RabbitMqExchange>(name => new RabbitMqExchange(name, this));
        }

        internal IConnection ListeningConnection => _listenerConnection ??= BuildConnection();
        internal IConnection SendingConnection => _sendingConnection ??= BuildConnection();

        /// <summary>
        /// Should Jasper attempt to auto-provision all exchanges, queues, and bindings
        /// if they do not already exist?
        /// </summary>
        public bool AutoProvision { get; set; }

        public bool AutoPurgeAllQueues { get; set; }

        public ConnectionFactory ConnectionFactory { get; } = new();

        public IList<AmqpTcpEndpoint> AmqpTcpEndpoints { get; } = new List<AmqpTcpEndpoint>();

        public LightweightCache<string, RabbitMqExchange> Exchanges { get; }

        public LightweightCache<string, RabbitMqQueue> Queues { get; } = new(name => new RabbitMqQueue(name));

        public IList<Binding> Bindings { get; } = new List<Binding>();

        public void Dispose()
        {
            _listenerConnection?.Close();
            _listenerConnection?.SafeDispose();

            _sendingConnection?.Close();
            _sendingConnection?.SafeDispose();
        }

        public IBindingExpression BindExchange(string exchangeName, Action<RabbitMqExchange>? configure = null)
        {
            DeclareExchange(exchangeName, configure);
            return new BindingExpression(exchangeName, this);
        }

        protected override IEnumerable<RabbitMqEndpoint> endpoints()
        {
            return _endpoints;
        }

        protected override RabbitMqEndpoint findEndpointByUri(Uri uri)
        {
            return _endpoints[uri];
        }

        public override ValueTask InitializeAsync(IJasperRuntime root)
        {
            if (AutoProvision)
            {
                InitializeAllObjects();
            }

            if (AutoPurgeAllQueues)
            {
                PurgeAllQueues();
            }

            return ValueTask.CompletedTask;
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
            var temp = new RabbitMqEndpoint(this) { QueueName = queueName };
            return findEndpointByUri(temp.Uri);
        }

        public RabbitMqEndpoint EndpointFor(string routingKey, string exchangeName)
        {
            var temp = new RabbitMqEndpoint(this)
            {
                RoutingKey = routingKey,
                ExchangeName = exchangeName
            };

            return findEndpointByUri(temp.Uri);
        }

        public RabbitMqEndpoint EndpointForExchange(string exchangeName)
        {
            var temp = new RabbitMqEndpoint(this) { ExchangeName = exchangeName };
            return findEndpointByUri(temp.Uri);
        }

        internal void InitializeEndpoint(RabbitMqEndpoint endpoint, IModel channel)
        {
            if (AutoProvision)
            {
                if (endpoint.ExchangeName.IsNotEmpty())
                {
                    var exchange = Exchanges[endpoint.ExchangeName];
                    exchange.Declare(channel!);
                }

                if (endpoint.QueueName.IsNotEmpty())
                {
                    var queue = Queues[endpoint.QueueName];
                    queue.Declare(channel!);

                    if (queue.PurgeOnStartup)
                    {
                        channel!.QueuePurge(queue.Name);
                    }
                }
            }
            else if (endpoint.QueueName.IsNotEmpty() && endpoint.QueueName.IsNotEmpty())
            {
                var queue = Queues[endpoint.QueueName];

                if (queue.PurgeOnStartup)
                {
                    channel!.QueuePurge(queue.Name);
                }
            }
        }

        internal class BindingExpression : IBindingExpression
        {
            private readonly string _exchangeName;
            private readonly RabbitMqTransport _parent;

            internal BindingExpression(string exchangeName, RabbitMqTransport parent)
            {
                _exchangeName = exchangeName;
                _parent = parent;
            }

            public IRabbitMqTransportExpression ToQueue(string queueName, Action<RabbitMqQueue>? configure = null,
                Dictionary<string, object>? arguments = null)
            {
                _parent.DeclareQueue(queueName, configure);
                ToQueue(queueName, $"{_exchangeName}_{queueName}");

                return _parent;
            }

            public IRabbitMqTransportExpression ToQueue(string queueName, string bindingKey,
                Action<RabbitMqQueue>? configure = null, Dictionary<string, object>? arguments = null)
            {
                _parent.DeclareQueue(queueName, configure);

                var binding = new Binding
                {
                    ExchangeName = _exchangeName,
                    BindingKey = bindingKey,
                    QueueName = queueName
                };

                if (arguments != null)
                {
                    binding.Arguments = arguments;
                }

                binding.AssertValid();

                _parent.Bindings.Add(binding);

                return _parent;
            }
        }
    }

    public interface IBindingExpression
    {
        /// <summary>
        ///     Bind the named exchange to a queue. The routing key will be
        ///     [exchange name]_[queue name]
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="configure">Optional configuration of the Rabbit MQ queue</param>
        /// <param name="arguments">Optional configuration for arguments to the Rabbit MQ binding</param>
        IRabbitMqTransportExpression ToQueue(string queueName, Action<RabbitMqQueue>? configure = null,
            Dictionary<string, object>? arguments = null);

        /// <summary>
        ///     Bind the named exchange to a queue with a user supplied binding key
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="bindingKey"></param>
        /// <param name="configure">Optional configuration of the Rabbit MQ queue</param>
        /// <param name="arguments">Optional configuration for arguments to the Rabbit MQ binding</param>
        IRabbitMqTransportExpression ToQueue(string queueName, string bindingKey,
            Action<RabbitMqQueue>? configure = null, Dictionary<string, object>? arguments = null);
    }
}
