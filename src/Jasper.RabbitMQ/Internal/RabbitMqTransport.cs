using System;
using System.Threading;
using Jasper.Configuration;
using Jasper.Messaging;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Sending;
using Microsoft.Extensions.Logging;

namespace Jasper.RabbitMQ.Internal
{
    public class RabbitMqEndpoint : Endpoint
    {
        public override void Parse(Uri uri)
        {
            throw new NotImplementedException();
        }
    }

    public class RabbitMqTransport : TransportBase<RabbitMqEndpoint>
    {
        public RabbitMqTransport(RabbitMqOptions options) : base("rabbitmq")
        {
        }

        protected override IListener createListener(RabbitMqEndpoint endpoint, IMessagingRoot root)
        {
            throw new NotImplementedException();
        }

        public override ISender CreateSender(Uri uri, CancellationToken cancellation, IMessagingRoot root)
        {
            throw new NotImplementedException();
        }

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
