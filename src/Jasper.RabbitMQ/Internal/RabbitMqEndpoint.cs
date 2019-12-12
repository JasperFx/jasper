using System;
using System.Linq;
using Baseline;
using Jasper.Configuration;
using Jasper.Messaging;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Sending;
using Jasper.Util;

namespace Jasper.RabbitMQ.Internal
{
    public class RabbitMqEndpoint : Endpoint
    {
        // TODO -- needs Protocol as well

        public string ExchangeName { get; private set; }
        public string RoutingKey { get; private set; }

        public RabbitMqEndpoint()
        {
        }

        public RabbitMqEndpoint(string exchangeName, string routingKey)
        {
            ExchangeName = exchangeName;
            if (ExchangeName.IsEmpty())
            {
                ExchangeName = TransportConstants.Default;
            }

            RoutingKey = routingKey;

            Uri = buildUri();
        }

        private Uri buildUri()
        {
            return new Uri($"{RabbitMqTransport.Protocol}://{ExchangeName}/{RoutingKey}");
        }



        public override Uri ReplyUri()
        {
            var uri = buildUri();
            if (!IsDurable)
            {
                return uri;
            }

            return $"{uri}/durable".ToUri();
        }

        public override void Parse(Uri uri)
        {
            if (uri.Scheme != RabbitMqTransport.Protocol)
            {
                throw new ArgumentOutOfRangeException($"This is not a rabbitmq Uri");
            }



            ExchangeName = uri.Host;
            if (ExchangeName.IsEmpty())
            {
                ExchangeName = TransportConstants.Default;
            }

            RoutingKey = uri.Segments.First(x => x != "/").Trim('/');

            Uri = buildUri();

            if (TransportConstants.Durable.EqualsIgnoreCase(uri.Segments.LastOrDefault()))
            {
                IsDurable = true;
            }


        }

        protected override void StartListening(IMessagingRoot root, ITransportRuntime runtime)
        {
            throw new NotImplementedException();
        }

        protected override ISender CreateSender(IMessagingRoot root)
        {
            throw new NotImplementedException();
        }
    }
}
