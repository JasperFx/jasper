using System;
using DotPulsar;
using DotPulsar.Abstractions;
using Jasper.Configuration;
using Jasper.Pulsar.Internal;
using Jasper.Runtime;
using Jasper.Transports.Sending;

namespace Jasper.Pulsar
{
    public class PulsarEndpoint : Endpoint
    {
        private PulsarTopic _topic;
        public PulsarTopic Topic
        {
            get => _topic;
            set
            {
                _topic = value;
                IsDurable = _topic.Persistence.Equals("persistent");
            }
        }
        public override Uri Uri => BuildUri();
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


        private Uri BuildUri(bool forReply = false)
        {
            return Topic.ToJasperUri(forReply);
        }

        public override void Parse(Uri uri)
        {
            Topic = uri;
        }

        protected internal override void StartListening(IMessagingRoot root, ITransportRuntime runtime)
        {
            if (!IsListener) return;

            var listener = new PulsarListener(this, root.TransportLogger, root.Cancellation);
            runtime.AddListener(listener, this);
        }

        protected override ISender CreateSender(IMessagingRoot root)
        {
            return new PulsarSender(this);
        }

        public override Uri ReplyUri() => BuildUri(true);
    }
}
