using System;
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

    public class RabbitMqAgent : IDisposable
    {
        private readonly object _locker = new object();
        private IConnection _connection;

        public RabbitMqAgent(string uriString) : this(uriString.ToUri())
        {
        }

        public RabbitMqAgent(Uri uri)
        {
            if (uri.Scheme != "rabbitmq")
                throw new ArgumentOutOfRangeException(nameof(uri), "The protocol must be 'rabbitmq'");
            Uri = uri;

            ConnectionFactory.Port = uri.IsDefaultPort ? 5672 : uri.Port;


            var segments = uri.Segments.Where(x => x != "/").Select(x => x.TrimEnd('/')).ToArray();
            if (!segments.Any())
                throw new ArgumentOutOfRangeException(nameof(uri),
                    "Unable to determine the routing key / queue for the Uri " + uri);

            if (segments[0] == TransportConstants.Durable)
            {
                IsDurable = uri.IsDurable();
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
                QueueName = segments.Skip(1).Join("/");
            }
            else
            {
                QueueName = segments.Single();
            }
        }

        public Uri Uri { get; }

        public AgentState State { get; private set; } = AgentState.Disconnected;

        public IModel Channel { get; private set; }

        public bool IsDurable { get; }
        public string ExchangeName { get; } = string.Empty;
        public ExchangeType ExchangeType { get; } = ExchangeType.Direct;
        public string QueueName { get; }

        public ConnectionFactory ConnectionFactory { get; } = new ConnectionFactory();

        public IEnvelopeMapper EnvelopeMapping { get; set; } = new DefaultEnvelopeMapper();

        public Func<IConnectionFactory, IConnection> ConnectionActivator { get; set; } = f => f.CreateConnection();


        public void Dispose()
        {
            Stop();
        }

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
            _connection = ConnectionActivator(ConnectionFactory);

            var channel = _connection.CreateModel();
            channel.CreateBasicProperties().Persistent = IsDurable;

            if (ExchangeName.IsNotEmpty())
            {
                channel.ExchangeDeclare(ExchangeName, ExchangeType.ToString().ToLowerInvariant(), IsDurable);
                channel.QueueDeclare(QueueName, IsDurable, autoDelete: false, exclusive: false);
                channel.QueueBind(QueueName, ExchangeName, "");
            }
            else
            {
                channel.QueueDeclare(QueueName, IsDurable, autoDelete: false, exclusive: false);
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
            return new PublicationAddress(ExchangeType.ToString(), ExchangeName, QueueName);
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
