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
using Jasper.Util;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ
{
    public enum AgentState
    {
        Connected,
        Disconnected
    }

    public enum ExchangeType
    {
        Direct,
        Fanout,
        Topic,
        Headers
    }

    public class RabbitMqEndpoint : Endpoint<IRabbitMqProtocol>
    {
        private readonly object _locker = new object();
        private IConnection _connection;


        public RabbitMqEndpoint(TransportUri uri, string connectionString) : base(uri, new DefaultRabbitMqProtocol())
        {
            if (uri.Protocol != "rabbitmq")
                throw new ArgumentOutOfRangeException(nameof(uri), "The protocol must be 'rabbitmq'");

            Uri = uri;

            var parts = connectionString.ToDelimitedArray(';');
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
                else if (key.EqualsIgnoreCase(nameof(Durable)))
                {
                    Durable = bool.Parse(value);
                }
                else if (key.EqualsIgnoreCase(nameof(Topic)))
                {
                    Topic = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(connectionString), $"Unknown connection string parameter '{key}'");
                }


            }

            if (Uri.QueueName.IsEmpty())
            {
                throw new ArgumentOutOfRangeException(nameof(connectionString), "Queue is required, but not specified");
            }

            if (Host.IsEmpty())
            {
                throw new ArgumentOutOfRangeException(nameof(connectionString), "Host is required, but not specified");
            }

        }

        public IConnection OpenConnection()
        {
            return AmqpTcpEndpoints.Any()
                ? ConnectionFactory.CreateConnection(AmqpTcpEndpoints)
                : ConnectionFactory.CreateConnection();
        }

        public ConnectionFactory ConnectionFactory { get; } = new ConnectionFactory();

        public IList<AmqpTcpEndpoint> AmqpTcpEndpoints { get; } = new List<AmqpTcpEndpoint>();



        public TransportUri Uri { get; }

        public string ExchangeName { get; } = string.Empty;
        public ExchangeType ExchangeType { get; } = ExchangeType.Direct;
        public string Queue { get; }

        public string Host
        {
            get => ConnectionFactory.HostName;
            private set => ConnectionFactory.HostName = value;
        }

        public int Port
        {
            get => ConnectionFactory.Port;
            set => ConnectionFactory.Port = value;
        }
        public string Topic { get; } = null;

        public bool Durable { get; }

        public Broker Broker { get; internal set; }

        public AgentState State { get; private set; } = AgentState.Disconnected;

        internal IModel Channel { get; private set; }


        public void Connect()
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
            _connection = OpenConnection();



            var channel = _connection.CreateModel();
            channel.CreateBasicProperties().Persistent = Durable;

            if (ExchangeName.IsNotEmpty())
            {
                channel.ExchangeDeclare(ExchangeName, ExchangeType.ToString().ToLowerInvariant(), Durable);
                channel.QueueDeclare(Queue, Durable, autoDelete: false, exclusive: false);

                // TODO -- routingKey is required for direct and topic
                channel.QueueBind(Queue, ExchangeName, "");
            }
            else
            {
                channel.QueueDeclare(Queue, Durable, autoDelete: false, exclusive: false);
            }

            Channel = channel;
        }

        public void Stop()
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

        public ISender CreateSender(ITransportLogger logger, CancellationToken cancellation)
        {
            return new RabbitMqSender(logger, this, cancellation);
        }

        public IListeningAgent CreateListeningAgent(Uri uri, JasperOptions options, ITransportLogger logger)
        {
            return new RabbitMqListeningAgent(uri, logger, Protocol, this);
        }

        public PublicationAddress PublicationAddress()
        {
            return new PublicationAddress(ExchangeType.ToString(), ExchangeName, Queue);
        }

        public Task Ping(Action<IModel> action)
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
