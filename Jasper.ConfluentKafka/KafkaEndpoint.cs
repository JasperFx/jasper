using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Confluent.Kafka;
using Jasper.Configuration;
using Jasper.ConfluentKafka.Internal;
using Jasper.Runtime;
using Jasper.Transports;
using Jasper.Transports.Sending;
using Jasper.Util;

namespace Jasper.ConfluentKafka
{
    public class KafkaEndpoint : Endpoint
    {
        private const string TopicToken = "topic";
        public string TopicName { get; set; }
        public ProducerConfig ProducerConfig { get; private set; }
        public override Uri Uri => BuildUri();
        private Uri BuildUri(bool forReply = false)
        {
            var list = new List<string>();

            if (TopicName.IsNotEmpty())
            {
                list.Add(TopicName);
                list.Add(TopicName.ToLowerInvariant());
            }

            if (forReply && IsDurable)
            {
                list.Add(TransportConstants.Durable);
            }

            var uri = $"{Protocols.Kafka}://{list.Join("/")}".ToUri();

            return uri;
        }

        public override void Parse(Uri uri)
        {
            if (uri.Scheme != Protocols.Kafka)
            {
                throw new ArgumentOutOfRangeException($"This is not a Kafka Transport Uri");
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
                if (segments.Peek().EqualsIgnoreCase(TopicToken))
                {
                    segments.Dequeue(); // token
                    TopicName = segments.Dequeue(); // value
                }
                else if (segments.Peek().EqualsIgnoreCase(TransportConstants.Durable))
                {
                    segments.Dequeue(); // token
                    IsDurable = true;
                }
                else
                {
                    throw new InvalidOperationException($"The Uri '{uri}' is invalid for a  Kafka Transport endpoint");
                }
            }
        }

        protected internal override void StartListening(IMessagingRoot root, ITransportRuntime runtime)
        {
            throw new NotImplementedException();
        }

        protected override ISender CreateSender(IMessagingRoot root)
        {
            throw new NotImplementedException();
        }

        public override Uri ReplyUri()
        {
            throw new NotImplementedException();
        }
    }

    public class KafkaEndpoint<TKey, TVal> : KafkaEndpoint
    {
        internal KafkaTransport<TKey, TVal> Parent { get; set; }

        protected internal override void StartListening(IMessagingRoot root, ITransportRuntime runtime)
        {
            throw new NotImplementedException();
        }

        protected override ISender CreateSender(IMessagingRoot root)
        {
            return new ConfluentKafkaSender<TKey, TVal>(this, Parent, root.TransportLogger, root.Cancellation);
        }
    }
}
