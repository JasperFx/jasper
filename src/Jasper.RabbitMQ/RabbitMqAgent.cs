using System;
using System.Linq;
using System.Threading;
using Baseline;
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
        public const string Protocol = "rabbitmq";

        public RabbitMqAgent(string uriString) : this (uriString.ToUri())
        {
        }

        public RabbitMqAgent(Uri uri)
        {
            if (uri.Scheme != "rabbitmq") throw new ArgumentOutOfRangeException(nameof(uri), "The protocol must be 'rabbitmq'");

            ConnectionFactory.Port = uri.IsDefaultPort ? 5672 : uri.Port;


            var segments = uri.Segments.Where(x => x != "/").Select(x => x.TrimEnd('/')).ToArray();
            if (!segments.Any()) throw new ArgumentOutOfRangeException(nameof(uri), "Unable to determine the routing key / queue for the Uri " + uri);

            if (segments[0] == TransportConstants.Durable)
            {
                IsDurable = uri.IsDurable();
                segments = segments.Skip(1).ToArray();
            }

            if (!segments.Any()) throw new ArgumentOutOfRangeException(nameof(uri), "Unable to determine the routing key / queue for the Uri " + uri);


            if (ExchangeType.TryParse<ExchangeType>(segments[0], out var exchangeType))
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

        }

        public bool IsDurable { get; }
        public string ExchangeName { get; }
        public ExchangeType ExchangeType { get; private set; } = ExchangeType.direct;
        public string QueueName { get; }

        public ConnectionFactory ConnectionFactory { get; } = new ConnectionFactory();

        public IEnvelopeMapper EnvelopeMapping { get; set; } = new DefaultEnvelopeMapper();

        public Func<IConnectionFactory, IConnection> ConnectionActivator { get; set; } = f => f.CreateConnection();


        // TODO -- when it initializes, it will need to create the queues
        // TODO -- when it initializes, it may need to create an exchange


        // TODO -- method to create a receiver
        // TODO -- method to create a sending agent


        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ISender CreateSender(CancellationToken cancellation)
        {
            // TODO -- will need to create a reply uri & listener here
            throw new NotImplementedException();
        }

        public IListeningAgent CreateListeningAgent(Uri uri, MessagingSettings settings)
        {
            throw new NotImplementedException();
        }
    }
}
