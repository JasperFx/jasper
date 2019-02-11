using System;
using System.Linq;
using System.Threading;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Receiving;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.RabbitMQ
{
    public class RabbitMqTransport : TransportBase
    {
        private readonly RabbitMqSettings _settings;

        public RabbitMqTransport(RabbitMqSettings rabbitMqSettings, IDurableMessagingFactory factory,
            ITransportLogger logger, JasperOptions settings)
            : base("rabbitmq", factory, logger, settings)
        {
            _settings = rabbitMqSettings;
        }

        protected override ISender createSender(Uri uri, CancellationToken cancellation)
        {
            var agent = _settings.ForEndpoint(uri);
            agent.Start();
            return agent.CreateSender(logger, cancellation);
        }

        protected override Uri[] validateAndChooseReplyChannel(Uri[] incoming)
        {
            var replies = _settings.ForEndpoint(_settings.ReplyUri);
            if (replies != null)
            {
                ReplyUri = replies.ToFullUri();
                return incoming.Concat(new Uri[] {replies.Uri}).Distinct().ToArray();
            }



            return incoming;
        }

        protected override IListeningAgent buildListeningAgent(Uri uri, JasperOptions settings)
        {
            var agent = _settings.ForEndpoint(uri);
            if (agent == null)
            {
                throw new ArgumentOutOfRangeException(nameof(uri), $"Could not resolve a Rabbit MQ endpoint for the Uri '{uri}'");
            }

            agent.Start();
            return agent.CreateListeningAgent(uri, settings, logger);
        }
    }
}
