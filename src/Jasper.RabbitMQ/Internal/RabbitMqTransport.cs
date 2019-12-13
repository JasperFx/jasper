using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly LightweightCache<Uri, RabbitMqEndpoint> _endpoints;

        public RabbitMqTransport() : base(Protocol)
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

        public ConnectionFactory ConnectionFactory { get; } = new ConnectionFactory();

        public IList<AmqpTcpEndpoint> AmqpTcpEndpoints { get; } = new List<AmqpTcpEndpoint>();

        public LightweightCache<string, RabbitMqExchange> Exchanges { get; }

        public LightweightCache<string, RabbitMqQueue> Queues { get; } = new LightweightCache<string, RabbitMqQueue>(name => new RabbitMqQueue(name));

        public IList<Binding> Bindings { get; } = new List<Binding>();

        public void AddBinding(Binding binding)
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

        public void DeclareAll()
        {
            using (var connection = BuildConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    foreach (var exchange in Exchanges)
                    {
                        exchange.Declare(channel);
                    }

                    foreach (var queue in Queues)
                    {
                        queue.Declare(channel);
                    }

                    foreach (var binding in Bindings)
                    {
                        binding.Declare(channel);
                    }
                }
            }
        }

        public void TeardownAll()
        {
            using (var connection = BuildConnection())
            {
                using (var channel = connection.CreateModel())
                {


                    foreach (var binding in Bindings)
                    {
                        binding.Teardown(channel);
                    }

                    foreach (var exchange in Exchanges)
                    {
                        exchange.Teardown(channel);
                    }

                    foreach (var queue in Queues)
                    {
                        queue.Teardown(channel);
                    }


                }
            }
        }

        public void PurgeAllQueues()
        {
            using (var connection = BuildConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    foreach (var queue in Queues)
                    {
                        queue.Purge(channel);
                    }
                }
            }
        }
    }

    public class RabbitMqQueue
    {
        public string Name { get; }

        public RabbitMqQueue(string name)
        {
            Name = name;
        }

        internal void Declare(IModel channel)
        {
            channel.QueueDeclare(Name, IsDurable, IsExclusive, AutoDelete, Arguments);
        }

        public bool AutoDelete { get; set; } = true;

        public bool IsExclusive { get; set; } = true;

        public bool IsDurable { get; set; } = false;

        public IDictionary<string, object> Arguments { get; } = new Dictionary<string, object>();

        public void Teardown(IModel channel)
        {
            channel.QueueDeleteNoWait(Name);
        }

        public void Purge(IModel channel)
        {
            try
            {
                channel.QueuePurge(Name);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to purge queue " + Name);
                Console.WriteLine(e);
            }
        }
    }



    public class Binding
    {
        public string BindingKey { get; set; }
        public string QueueName { get; set; }
        public string ExchangeName { get; set; }

        public IDictionary<string, object> Arguments { get; set; } = new Dictionary<string, object>();

        internal void Declare(IModel channel)
        {
            channel.QueueBind(QueueName, ExchangeName, BindingKey, Arguments);
        }

        public void Teardown(IModel channel)
        {
            channel.QueueUnbind(QueueName, ExchangeName, BindingKey, Arguments);
        }

        internal void AssertValid()
        {
            if (BindingKey.IsEmpty() || QueueName.IsEmpty() || ExchangeName.IsEmpty())
            {
                throw new InvalidOperationException($"{nameof(BindingKey)} properties {nameof(BindingKey)}, {nameof(QueueName)}, and {nameof(ExchangeName)} are all required for this operation");
            }
        }
    }
}
