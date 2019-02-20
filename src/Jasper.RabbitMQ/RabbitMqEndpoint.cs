using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Receiving;
using Jasper.Messaging.Transports.Sending;
using Jasper.RabbitMQ.Internal;
using Jasper.Util;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ
{
    public class RabbitMqEndpoint : Endpoint<IRabbitMqProtocol>
    {
        private readonly object _locker = new object();
        private IConnection _connection;


        public RabbitMqEndpoint(TransportUri uri, string connectionString) : base(uri, new DefaultRabbitMqProtocol())
        {
            if (uri.Protocol != "rabbitmq")
                throw new ArgumentOutOfRangeException(nameof(uri), "The protocol must be 'rabbitmq'");

            Port = 5672;

            TransportUri = uri;

            var parts = connectionString.Trim(';').ToDelimitedArray(';');
            foreach (var part in parts)
            {
                var keyValues = part.Split('=');
                if (keyValues.Length != 2) throw new ArgumentOutOfRangeException(nameof(connectionString), "The connection string is malformed");

                var key = keyValues[0];
                var value = keyValues[1];

                if (key.EqualsIgnoreCase(nameof(Host)))
                {
                    Host = value;
                }
                else if (key.EqualsIgnoreCase(nameof(Port)))
                {
                    Port = int.Parse(value);
                }
                else if (key.EqualsIgnoreCase(nameof(ExchangeName)))
                {
                    ExchangeName = value;
                }
                else if (key.EqualsIgnoreCase(nameof(ExchangeType)))
                {
                    ExchangeType = (ExchangeType) Enum.Parse(typeof(ExchangeType), value, true);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(connectionString), $"Unknown connection string parameter '{key}'");
                }


            }

            if (TransportUri.QueueName.IsEmpty())
            {
                throw new ArgumentOutOfRangeException(nameof(connectionString), "Queue is required, but not specified");
            }

            if (Host.IsEmpty())
            {
                throw new ArgumentOutOfRangeException(nameof(connectionString), "Host is required, but not specified");
            }

        }

        /// <summary>
        /// Configure how the connection to Rabbit MQ will be created, including authentication information
        /// </summary>
        public ConnectionFactory ConnectionFactory { get; } = new ConnectionFactory();

        /// <summary>
        /// Override the server and port bindings of any opened connections
        /// </summary>
        public IList<AmqpTcpEndpoint> AmqpTcpEndpoints { get; } = new List<AmqpTcpEndpoint>();



        /// <summary>
        /// Information about the configured Rabbit MQ endpoint
        /// </summary>
        public TransportUri TransportUri { get; }

        /// <summary>
        /// The Rabbit MQ exchange name, maybe be null or empty
        /// </summary>
        public string ExchangeName { get; } = string.Empty;

        /// <summary>
        /// The type of exchange this endpoint will try to bind to
        /// </summary>
        public ExchangeType ExchangeType { get; } = ExchangeType.Direct;

        /// <summary>
        /// ConnectionFactory.HostName
        /// </summary>
        public string Host
        {
            get => ConnectionFactory.HostName;
            private set => ConnectionFactory.HostName = value;
        }

        /// <summary>
        /// ConnectionFactory.Port
        /// </summary>
        public int Port
        {
            get => ConnectionFactory.Port;
            set => ConnectionFactory.Port = value;
        }

        public string Topic => Uri.TopicName;



        internal AgentState State { get; private set; } = AgentState.Disconnected;

        internal IModel Channel { get; private set; }


        internal void Connect()
        {
            lock (_locker)
            {
                if (State == AgentState.Connected) return;

                startNewConnection();

                State = AgentState.Connected;
            }
        }

        private void startNewConnection()
        {
            _connection = AmqpTcpEndpoints.Any()
                ? ConnectionFactory.CreateConnection(AmqpTcpEndpoints)
                : ConnectionFactory.CreateConnection();



            var channel = _connection.CreateModel();
            channel.CreateBasicProperties().Persistent = TransportUri.Durable;

            if (ExchangeName.IsNotEmpty())
            {
                channel.ExchangeDeclare(ExchangeName, ExchangeType.ToString().ToLowerInvariant(), TransportUri.Durable);
                channel.QueueDeclare(TransportUri.QueueName, TransportUri.Durable, autoDelete: false, exclusive: false);

                if (ExchangeType == ExchangeType.Topic)
                {
                    if (TransportUri.TopicName.IsEmpty())
                    {
                        throw new InvalidOperationException($"Topic name is required to connect to a topic exchange. Invalid Uri is '{TransportUri.ToUri()}'");
                    }

                    channel.QueueBind(TransportUri.QueueName, ExchangeName, TransportUri.TopicName);
                }
                else
                {
                    channel.QueueBind(TransportUri.QueueName, ExchangeName, TransportUri.RoutingKey ?? "");
                }


            }
            else
            {
                channel.QueueDeclare(TransportUri.QueueName, TransportUri.Durable, autoDelete: false, exclusive: false);
            }

            Channel = channel;
        }

        internal void Stop()
        {
            lock (_locker)
            {
                if (State == AgentState.Disconnected) return;

                teardownConnection();
            }
        }

        private void teardownConnection()
        {
            Channel.Abort();
            Channel.Dispose();
            _connection.Close();
            _connection.Dispose();

            Channel = null;
            _connection = null;

            State = AgentState.Disconnected;
        }

        public override void Dispose()
        {
            Stop();
        }

        internal ISender CreateSender(ITransportLogger logger, CancellationToken cancellation)
        {
            return new RabbitMqSender(logger, this, cancellation);
        }

        internal IListeningAgent CreateListeningAgent(Uri uri, JasperOptions options, ITransportLogger logger)
        {
            return new RabbitMqListeningAgent(uri, logger, Protocol, this);
        }

        internal PublicationAddress PublicationAddress()
        {
            return new PublicationAddress(ExchangeType.ToString(), ExchangeName, TransportUri.QueueName);
        }

        internal Task Ping(Action<IModel> action)
        {
            lock (_locker)
            {
                if (State == AgentState.Connected) return Task.CompletedTask;

                startNewConnection();


                try
                {
                    action(Channel);
                }
                catch (Exception)
                {
                    teardownConnection();
                    throw;
                }
            }


            return Task.CompletedTask;
        }
    }
}
