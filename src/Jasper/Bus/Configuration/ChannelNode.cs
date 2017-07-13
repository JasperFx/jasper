using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Routing;

namespace Jasper.Bus.Configuration
{
    public enum DeliveryMode
    {
        /// <summary>
        /// If supported by the transport, this opts into guaranteed delivery mechanics for this channel
        /// </summary>
        DeliveryGuaranteed,

        /// <summary>
        /// If supported by the transport, this opts into a faster "fire and forget" mechanism for sending and receiving messages. Use this option for control channels.
        /// </summary>
        DeliveryFastWithoutGuarantee
    }

    public class ChannelNode : IContentTypeAware
    {
        public Uri Uri { get; internal set; }

        public ChannelNode(Uri uri)
        {
            Uri = uri;
        }

        public readonly List<string> AcceptedContentTypes = new List<string>{"application/json"};

        IEnumerable<string> IContentTypeAware.Accepts => AcceptedContentTypes;
        public bool Incoming { get; set; }
        public string DefaultContentType => AcceptedContentTypes.FirstOrDefault();

        public IList<IRoutingRule> Rules = new List<IRoutingRule>();

        public bool ShouldSendMessage(Type messageType)
        {
            return Rules.Any(x => x.Matches(messageType));
        }

        public Uri ReplyUri { get; set; }
        public Uri Destination { get; set; }

        public ISender Sender { get; set; }
        public DeliveryMode Mode { get; set; } = DeliveryMode.DeliveryGuaranteed;
        public IList<IEnvelopeModifier> Modifiers { get; } = new List<IEnvelopeModifier>();

        public int MaximumParallelization { get; set; } = 5;

        public void ApplyModifiers(Envelope envelope)
        {
            foreach (var modifier in Modifiers)
            {
                modifier.Modify(envelope);
            }
        }
    }

    // Use a nullo if need be?
    public interface ISender
    {
        Task Send(Envelope envelope);
    }

    public class NulloSender : ISender
    {
        private readonly ITransport _transport;
        private readonly Uri _destination;

        public NulloSender(ITransport transport, Uri destination)
        {
            _transport = transport;
            _destination = destination;
        }

        public Task Send(Envelope envelope)
        {
            return _transport.Send(envelope, _destination);
        }
    }
}
