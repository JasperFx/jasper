using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Baseline;
using Jasper.Configuration;
using Jasper.Runtime;
using Jasper.Transports;
using Jasper.Transports.Sending;
using Jasper.Util;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public class RabbitMqEndpoint : TransportEndpoint<IBasicProperties>, IDisposable
    {
        public const string Queue = "queue";
        public const string Exchange = "exchange";
        public const string Routing = "routing";

        private IListener _listener;

        public RabbitMqEndpoint()
        {
            MapProperty(x => x.CorrelationId, (e, p) => e.CorrelationId = p.CorrelationId, (e,p) => p.CorrelationId = e.CorrelationId);
            MapProperty(x => x.ContentType, (e, p) => e.ContentType = p.ContentType, (e,p) => p.ContentType = e.ContentType);
        }

        public string ExchangeName { get; set; } = string.Empty;
        public string RoutingKey { get; set; }

        public string QueueName { get; set; }

        public int ListenerCount { get; set; } = 0;

        public override IDictionary<string, object> DescribeProperties()
        {
            var dict = base.DescribeProperties();

            if (ExchangeName.IsNotEmpty())
            {
                dict.Add(nameof(ExchangeName), ExchangeName);
            }

            if (RoutingKey.IsNotEmpty())
            {
                dict.Add(nameof(RoutingKey), RoutingKey);
            }

            if (QueueName.IsNotEmpty())
            {
                dict.Add(nameof(QueueName), QueueName);
            }

            if (ListenerCount > 0 && IsListener)
            {
                dict.Add(nameof(ListenerCount), ListenerCount);
            }

            // TODO -- there will be more here as we allow the rabbit connection to vary more

            return dict;
        }

        internal RabbitMqTransport Parent { get; set; }

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
            return Mode == EndpointMode.Durable ? $"{Uri}/durable".ToUri() : Uri;
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
                    Mode = EndpointMode.Durable;
                }
                else
                {
                    throw new InvalidOperationException($"The Uri '{uri}' is invalid for a Rabbit MQ endpoint");
                }
            }


        }

        public override void StartListening(IMessagingRoot root, ITransportRuntime runtime)
        {
            if (!IsListener) return;

            if (ListenerCount > 1)
            {
                _listener = new ParallelRabbitMqListener(root.TransportLogger, this, Parent);
            }
            else
            {
                _listener = new RabbitMqListener(root.TransportLogger, this, Parent);
            }

            runtime.AddListener(_listener, this);
        }

        protected override ISender CreateSender(IMessagingRoot root)
        {
            return new RabbitMqSender(this, this.Parent);
        }

        public void Dispose()
        {
            _listener?.Dispose();
        }

        protected override void writeOutgoingHeader(IBasicProperties outgoing, string key, string value)
        {
            outgoing.Headers[key] = value;
        }

        protected override bool tryReadIncomingHeader(IBasicProperties incoming, string key, out string value)
        {
            if (incoming.Headers.TryGetValue(key, out var raw))
            {
                value = raw is byte[] b ? Encoding.Default.GetString(b) : raw.ToString();
                return true;
            }

            value = null;
            return false;
        }

        protected override void writeIncomingHeaders(IBasicProperties incoming, Envelope envelope)
        {
            foreach (var pair in incoming.Headers)
            {
                envelope.Headers[pair.Key] = pair.Value is byte[] b ? Encoding.Default.GetString(b) : pair.Value?.ToString();
            }
        }
    }


}
