using System;
using System.Collections.Generic;
using System.Threading;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public class RabbitMqTransport : TransportBase<RabbitMqEndpoint>
    {
        public const string Protocol = "rabbitmq";

        public RabbitMqTransport() : base(Protocol)
        {
        }

        protected override IEnumerable<RabbitMqEndpoint> endpoints()
        {
            throw new NotImplementedException();
        }

        protected override RabbitMqEndpoint findEndpointByUri(Uri uri)
        {
            throw new NotImplementedException();
        }

        public ConnectionFactory ConnectionFactory { get; } = new ConnectionFactory();



//        protected override ISender buildSender(TransportUri transportUri, RabbitMqEndpoint endpoint,
//            CancellationToken cancellation, IMessagingRoot root)
//        {
//            endpoint.Connect();
//            return endpoint.CreateSender(root.TransportLogger, cancellation);
//        }
//
//        protected override IListener buildListeningAgent(TransportUri transportUri, RabbitMqEndpoint endpoint,
//            AdvancedSettings settings, HandlerGraph handlers, IMessagingRoot root)
//        {
//            if (endpoint == null)
//            {
//                throw new ArgumentOutOfRangeException(nameof(transportUri), $"Could not resolve a Rabbit MQ endpoint for the Uri '{transportUri}'");
//            }
//
//            if (transportUri.IsMessageSpecificTopic())
//            {
//                return new RabbitMqMessageSpecificTopicListener(endpoint, handlers, transportUri, root.TransportLogger, settings);
//            }
//            else
//            {
//                endpoint.Connect();
//                return endpoint.CreateListeningAgent(transportUri.ToUri(), settings, root.TransportLogger);
//            }
//
//
//        }
    }
}
