using System;
using System.Linq;
using System.Threading;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Receiving;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.RabbitMQ
{
    public class RabbitMqTransport : ExternalTransportBase<RabbitMqSettings, RabbitMqEndpoint>
    {
        private readonly RabbitMqSettings _settings;

        public RabbitMqTransport(RabbitMqSettings settings, IDurableMessagingFactory factory, ITransportLogger logger, JasperOptions jasperOptions) : base("rabbitmq", settings, factory, logger, jasperOptions)
        {
            _settings = settings;
        }

        protected override ISender buildSender(TransportUri transportUri, RabbitMqEndpoint endpoint, CancellationToken cancellation)
        {
            endpoint.Connect();
            return endpoint.CreateSender(logger, cancellation);
        }

        protected override IListeningAgent buildListeningAgent(TransportUri transportUri, RabbitMqEndpoint endpoint, JasperOptions settings)
        {
            if (endpoint == null)
            {
                throw new ArgumentOutOfRangeException(nameof(transportUri), $"Could not resolve a Rabbit MQ endpoint for the Uri '{transportUri}'");
            }

            endpoint.Connect();
            return endpoint.CreateListeningAgent(transportUri.ToUri(), settings, logger);
        }
    }
}
