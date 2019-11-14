using System;
using System.Linq;
using System.Threading;
using Jasper.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.Messaging.Transports
{
    public abstract class ExternalTransportBase<TSettings, TEndpoint> : TransportBase where TSettings : ExternalTransportSettings<TEndpoint> where TEndpoint : class
    {
        private readonly TSettings _options;

        public ExternalTransportBase(string protocol, TSettings options)
            : base(protocol)
        {
            _options = options;
        }

        protected sealed override ISender createSender(Uri uri, CancellationToken cancellation, IMessagingRoot root)
        {
            var transportUri = new TransportUri(uri);
            var endpoint = _options.For(transportUri);

            if (endpoint == null) throw new ArgumentOutOfRangeException(nameof(uri), $"Unknown {Protocol} connection named '{transportUri.ConnectionName}'");


            return buildSender(transportUri, endpoint, cancellation, root);
        }

        protected abstract ISender buildSender(TransportUri transportUri, TEndpoint endpoint,
            CancellationToken cancellation, IMessagingRoot root);

        protected override IListener buildListeningAgent(ListenerSettings listenerSettings,
            AdvancedSettings settings,
            HandlerGraph handlers, IMessagingRoot root)
        {
            var transportUri = new TransportUri(listenerSettings.Uri);
            var endpoint = _options.For(transportUri);

            if (endpoint == null) throw new ArgumentOutOfRangeException(nameof(listenerSettings), $"Unknown {Protocol} connection named '{transportUri.ConnectionName}'");

            return buildListeningAgent(transportUri, endpoint, settings, handlers, root);
        }

        protected abstract IListener buildListeningAgent(TransportUri transportUri, TEndpoint endpoint,
            AdvancedSettings settings, HandlerGraph handlers, IMessagingRoot root);

        protected override ListenerSettings[] validateAndChooseReplyChannel(ListenerSettings[] incoming)
        {
            if (_options.ReplyUri == null) return incoming;

            var replies = _options.For(_options.ReplyUri);
            if (replies != null)
            {
                ReplyUri = _options.ReplyUri.ToUri();
                return incoming.Concat(new ListenerSettings[] {new ListenerSettings{Uri = ReplyUri}}).Distinct().ToArray();
            }

            return incoming;
        }



    }
}
