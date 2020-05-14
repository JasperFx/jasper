using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Configuration;
using Jasper.Runtime;
using Jasper.Transports;
using Jasper.Transports.Sending;
using Jasper.Util;

namespace Jasper.RabbitMQ.Internal
{
    public class RabbitMqEndpoint : Endpoint
    {
        public const string Queue = "queue";
        public const string Exchange = "exchange";
        public const string Routing = "routing";

        public string ExchangeName { get; set; } = string.Empty;
        public string RoutingKey { get; set; }

        public string QueueName { get; set; }
        internal RabbitMqTransport Parent { get; set; }

        public IRabbitMqProtocol Protocol { get; set; } = new DefaultRabbitMqProtocol();

        public RabbitMqEndpoint()
        {
        }

        public override Uri Uri
        {
            get
            {
                var list = new List<string>();

                if (QueueName.IsNotEmpty())
                {
                    list.Add(Queue);
                    list.Add(QueueName.ToLowerInvariant());
                }
                else
                {
                    if (ExchangeName.IsNotEmpty())
                    {
                        list.Add(Exchange);
                        list.Add(ExchangeName.ToLowerInvariant());
                    }

                    if (RoutingKey.IsNotEmpty())
                    {
                        list.Add(Routing);
                        list.Add(RoutingKey.ToLowerInvariant());
                    }
                }



                var uri = $"{RabbitMqTransport.ProtocolName}://{list.Join("/")}".ToUri();

                return uri;
            }
        }


        public override Uri ReplyUri()
        {
            return IsDurable ? $"{Uri}/durable".ToUri() : Uri;
        }

        public override void Parse(Uri uri)
        {
            if (uri.Scheme != RabbitMqTransport.ProtocolName)
            {
                throw new ArgumentOutOfRangeException($"This is not a rabbitmq Uri");
            }

            var raw = uri.Segments.Where(x => x != "/").Select(x => x.Trim('/'));
            var segments = new Queue<string>();
            segments.Enqueue(uri.Host);
            foreach (var segment in raw)
            {
                segments.Enqueue(segment);
            }


            while (segments.Any())
            {
                if (segments.Peek().EqualsIgnoreCase(Exchange))
                {
                    segments.Dequeue();
                    ExchangeName = segments.Dequeue();
                }
                else if (segments.Peek().EqualsIgnoreCase(Queue))
                {
                    segments.Dequeue();
                    QueueName = segments.Dequeue();
                }
                else if (segments.Peek().EqualsIgnoreCase(Routing))
                {
                    segments.Dequeue();
                    RoutingKey = segments.Dequeue();
                }
                else if (segments.Peek().EqualsIgnoreCase(TransportConstants.Durable))
                {
                    segments.Dequeue();
                    IsDurable = true;
                }
                else
                {
                    throw new InvalidOperationException($"The Uri '{uri}' is invalid for a Rabbit MQ endpoint");
                }
            }


        }

        protected internal override void StartListening(IMessagingRoot root, ITransportRuntime runtime)
        {
            if (!IsListener) return;

            var listener = new RabbitMqListener(root.TransportLogger, this, Parent);
            runtime.AddListener(listener, this);
        }

        protected override ISender CreateSender(IMessagingRoot root)
        {
            return new RabbitMqSender(this, this.Parent);
        }
    }


}
