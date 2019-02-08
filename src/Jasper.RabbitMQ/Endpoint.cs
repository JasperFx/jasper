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

    public class Endpoint : IDisposable
    {
        private readonly object _locker = new object();
        private IConnection _connection;

        public Endpoint(Uri uri)
        {
            if (uri.Scheme != "rabbitmq")
                throw new ArgumentOutOfRangeException(nameof(uri), "The protocol must be 'rabbitmq'");

            Uri = uri;


            BrokerUri = uri.IsDefaultPort
                ? new Uri($"rabbitmq://{uri.Host}:5672")
                : new Uri($"rabbitmq://{uri.Host}:{uri.Port}");


            var segments = uri.Segments.Where(x => x != "/").Select(x => x.TrimEnd('/')).ToArray();
            if (!segments.Any())
                throw new ArgumentOutOfRangeException(nameof(uri),
                    "Unable to determine the routing key / queue for the Uri " + uri);

            if (segments[0] == TransportConstants.Durable)
            {
                Durable = uri.IsDurable();
                segments = segments.Skip(1).ToArray();
            }

            if (!segments.Any())
                throw new ArgumentOutOfRangeException(nameof(uri),
                    "Unable to determine the routing key / queue for the Uri " + uri);


            if (Enum.TryParse<ExchangeType>(segments[0], true, out var exchangeType))
            {
                ExchangeType = exchangeType;
                segments = segments.Skip(1).ToArray();
            }

            if (!segments.Any())
                throw new ArgumentOutOfRangeException(nameof(uri),
                    "Unable to determine the routing key / queue for the Uri " + uri);


            if (segments.Length > 1)
            {
                ExchangeName = segments[0];
                Queue = segments.Skip(1).Join("/");
            }
            else
            {
                Queue = segments.Single();
            }
        }

        public Endpoint(string name, string connectionString)
        {
            Uri = new Uri("rabbitmq://" + name);

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
                else if (key.EqualsIgnoreCase(nameof(Queue)))
                {
                    Queue = value;
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

            if (Queue.IsEmpty())
            {
                throw new ArgumentOutOfRangeException(nameof(connectionString), "Queue is required, but not specified");
            }

            if (Host.IsEmpty())
            {
                throw new ArgumentOutOfRangeException(nameof(connectionString), "Host is required, but not specified");
            }

            BrokerUri = new Uri($"rabbitmq://{Host}:{Port}");
        }

        public Uri ToFullUri()
        {
            var segments = new List<string>();

            if (Durable)
            {
                segments.Add(TransportConstants.Durable);
            }

            segments.Add(ExchangeType.ToString().ToLower());

            if (ExchangeName.IsNotEmpty())
            {
                segments.Add(ExchangeName);
            }

            segments.Add(Queue);

            return new Uri($"{BrokerUri}{segments.Join("/")}");
        }

        public Uri Uri { get; }

        public Uri BrokerUri { get; }

        public string ExchangeName { get; } = string.Empty;
        public ExchangeType ExchangeType { get; } = ExchangeType.Direct;
        public string Queue { get; }
        public string Host { get; }
        public int Port { get; } = 5672;
        public string Topic { get; } = null;

        public bool Durable { get; }

        public IEnvelopeMapper EnvelopeMapping { get; set; } = new DefaultEnvelopeMapper();


        // NEW STUFF FROM AGENT HERE______

        public Broker Broker { get; internal set; }

        public AgentState State { get; private set; } = AgentState.Disconnected;

        internal IModel Channel { get; private set; }


        public void Start()
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
            _connection = Broker.OpenConnection();



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

        public void Dispose()
        {
            Stop();
        }

        public ISender CreateSender(ITransportLogger logger, CancellationToken cancellation)
        {
            // TODO -- will need to create a reply uri & listener here
            return new RabbitMqSender(logger, this, cancellation);
        }

        public IListeningAgent CreateListeningAgent(Uri uri, JasperOptions settings, ITransportLogger logger)
        {
            return new RabbitMqListeningAgent(uri, logger, EnvelopeMapping, this);
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
