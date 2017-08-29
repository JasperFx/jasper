using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Routing;

namespace Jasper.Bus
{
    public interface IChannel
    {
        Uri Uri { get; }
        Uri ReplyUri { get; set; }
        Uri Destination { get; set; }

        // TODO -- just bake this into IChannel. That'd allow
        // you to bring the modifier stuff internally too
        ISender Sender { get; set; }
        void ApplyModifiers(Envelope envelope);
    }

    public class ChannelNode : IChannel
    {
        public Uri Uri { get; internal set; }

        public ChannelNode(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            Uri = uri;
            Destination = uri;
        }

        public bool Incoming { get; set; }

        public IList<IRoutingRule> Rules = new List<IRoutingRule>();

        public bool ShouldSendMessage(Type messageType)
        {
            return Rules.Any(x => x.Matches(messageType));
        }

        public Uri ReplyUri { get; set; }
        public Uri Destination { get; set; }

        public ISender Sender { get; set; }
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
