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
        public const string Queue = "queue";
        public const string Exchange = "exchange";
        public const string Routing = "routing";

        public string ExchangeName { get; set; }
        public string RoutingKey { get; set; }

        public string QueueName { get; set; }
        internal RabbitMqTransport Parent { get; set; }

        public IRabbitMqProtocol Protocol { get; set; } = new DefaultRabbitMqProtocol();

        public RabbitMqEndpoint()
        {
        }

        public override Uri Uri { get; }


        internal static Uri ToUri(string exchangeName, string routingKey)
        {
            return new Uri($"{RabbitMqTransport.ProtocolName}://{exchangeName}/{routingKey}");
        }

        private Uri buildUri()
        {
            return ToUri(ExchangeName, RoutingKey);
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
            if (uri.Scheme != RabbitMqTransport.ProtocolName)
            {
                throw new ArgumentOutOfRangeException($"This is not a rabbitmq Uri");
            }

            ExchangeName = uri.Host;
            if (ExchangeName.IsEmpty())
            {
                ExchangeName = TransportConstants.Default;
            }

            RoutingKey = uri.Segments.First(x => x != "/").Trim('/');


            if (TransportConstants.Durable.EqualsIgnoreCase(uri.Segments.LastOrDefault()))
            {
                IsDurable = true;
            }


        }

        protected override void StartListening(IMessagingRoot root, ITransportRuntime runtime)
        {
            if (!IsListener) return;

            var listener = new RabbitMqListener(root.TransportLogger, this, Parent);
            runtime.AddListener(listener, this);
        }

        protected override ISender CreateSender(IMessagingRoot root)
        {
            return new RabbitMqSender(root.TransportLogger, this, Parent, root.Cancellation);
        }
    }
}
