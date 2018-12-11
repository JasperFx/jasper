using System;
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
            var agent = _settings.For(uri);
            agent.Start();
            return agent.CreateSender(logger, cancellation);
        }

        protected override Uri[] validateAndChooseReplyChannel(Uri[] incoming)
        {
            return incoming;
        }

        protected override IListeningAgent buildListeningAgent(Uri uri, JasperOptions settings)
        {
            var agent = _settings.For(uri);
            agent.Start();
            return agent.CreateListeningAgent(uri, settings, logger);
        }
    }
}
