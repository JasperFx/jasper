using System;
using DotPulsar;
using DotPulsar.Abstractions;
using Jasper.Configuration;
using Jasper.Pulsar.Internal;
using Jasper.Runtime;
using Jasper.Transports;
using Jasper.Transports.Sending;

namespace Jasper.Pulsar
{
    public class PulsarEndpoint : Endpoint
    {
        public PulsarTopic Topic { get; set; }

        public override Uri Uri => BuildUri(false);
        public ConsumerOptions ConsumerOptions { get; set; }
        public ProducerOptions ProducerOptions { get; set; }
        public IPulsarClient PulsarClient { get; set; }

        public PulsarEndpoint()
        {
            
        }

        public PulsarEndpoint(string topic)
        {
            Topic = topic;
        }

        public PulsarEndpoint(Uri uri) : base(uri)
        {
            Topic = uri;
        }

        public override void Parse(Uri uri)
        {
            IsDurable = uri.ToString().EndsWith(TransportConstants.Durable);
            var url = uri.ToString();
            string pulsarTopic = url.Substring(0, url.Length - (IsDurable ? TransportConstants.Durable.Length + 1 : 0));

            Topic = new PulsarTopic(pulsarTopic);
        }

        private Uri BuildUri(bool forReply = false)
        {
            if (forReply && IsDurable)
            {
                return new Uri(Topic + $"/{TransportConstants.Durable}");
            }

            return Topic;
        }

        protected internal override void StartListening(IMessagingRoot root, ITransportRuntime runtime)
        {
            if (!IsListener) return;

            var listener = new PulsarListener(this, root.TransportLogger, root.Cancellation);
            runtime.AddListener(listener, this);
        }

        protected override ISender CreateSender(IMessagingRoot root)
        {
            return new PulsarSender(this, root.Cancellation);
        }

        public override Uri ReplyUri() => BuildUri(true);
    }
}
