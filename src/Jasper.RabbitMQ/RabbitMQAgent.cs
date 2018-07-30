using System;
using System.Linq;
using System.Threading;
using Baseline;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.Transports.Receiving;
using Jasper.Messaging.Transports.Sending;
using Jasper.Util;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ
{
    public class RabbitMqAgent : IDisposable
    {
        private readonly Lazy<IConnection> _connection;
        private readonly Lazy<IModel> _model;

        public Uri Uri { get; }

        public RabbitMqAgent(string uriString) : this (uriString.ToUri())
        {
        }

        public RabbitMqAgent(Uri uri)
        {
            if (uri.Scheme != "rabbitmq") throw new ArgumentOutOfRangeException(nameof(uri), "The protocol must be 'rabbitmq'");
            Uri = uri;

            ConnectionFactory.Port = uri.IsDefaultPort ? 5672 : uri.Port;


            var segments = uri.Segments.Where(x => x != "/").Select(x => x.TrimEnd('/')).ToArray();
            if (!segments.Any()) throw new ArgumentOutOfRangeException(nameof(uri), "Unable to determine the routing key / queue for the Uri " + uri);

            if (segments[0] == TransportConstants.Durable)
            {
                IsDurable = uri.IsDurable();
                segments = segments.Skip(1).ToArray();
            }

            if (!segments.Any()) throw new ArgumentOutOfRangeException(nameof(uri), "Unable to determine the routing key / queue for the Uri " + uri);


            if (Enum.TryParse<ExchangeType>(segments[0], out var exchangeType))
            {
                ExchangeType = exchangeType;
                segments = segments.Skip(1).ToArray();
            }

            if (!segments.Any()) throw new ArgumentOutOfRangeException(nameof(uri), "Unable to determine the routing key / queue for the Uri " + uri);


            if (segments.Length > 1)
            {
                ExchangeName = segments[0];
                QueueName = segments.Skip(1).Join("/");
            }
            else
            {
                QueueName = segments.Single();
            }

            _connection = new Lazy<IConnection>(() => ConnectionActivator(ConnectionFactory));

            _model = new Lazy<IModel>(() =>
            {
                var channel = _connection.Value.CreateModel();
                channel.CreateBasicProperties().Persistent = IsDurable;

                if (ExchangeName.IsNotEmpty())
                {
                    channel.ExchangeDeclare(ExchangeName, ExchangeType.ToString(), IsDurable, false);
                    channel.QueueDeclare(QueueName, durable: IsDurable, autoDelete: false, exclusive: false);
                    channel.QueueBind(QueueName, ExchangeName, "");
                }
                else
                {
                    channel.QueueDeclare(QueueName, durable: IsDurable, autoDelete: false, exclusive: false);
                }

                return channel;
            });

        }

        public bool IsDurable { get; }
        public string ExchangeName { get; } = string.Empty;
        public ExchangeType ExchangeType { get; private set; } = ExchangeType.direct;
        public string QueueName { get; }

        public ConnectionFactory ConnectionFactory { get; } = new ConnectionFactory();

        public IEnvelopeMapper EnvelopeMapping { get; set; } = new DefaultEnvelopeMapper();

        public Func<IConnectionFactory, IConnection> ConnectionActivator { get; set; } = f => f.CreateConnection();



        public void Dispose()
        {
            if (_model.IsValueCreated)
            {
                _model.Value.Dispose();
            }

            if (_connection.IsValueCreated)
            {
                _connection.Value.Dispose();
            }
        }

        public ISender CreateSender(ITransportLogger logger, CancellationToken cancellation)
        {
            // TODO -- will need to create a reply uri & listener here
            return new RabbitMQSender(logger, this, _model.Value, cancellation);
        }

        public IListeningAgent CreateListeningAgent(Uri uri, MessagingSettings settings, ITransportLogger logger)
        {
            return new RabbitMQListeningAgent(uri, logger, _model.Value, EnvelopeMapping, this);
        }

        public PublicationAddress PublicationAddress()
        {
            return new PublicationAddress(ExchangeType.ToString(), ExchangeName, QueueName);
        }
    }
}
