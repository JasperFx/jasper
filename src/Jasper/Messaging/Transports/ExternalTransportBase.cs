using System;
using System.Linq;
using System.Threading;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Transports.Receiving;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.Messaging.Transports
{
    public abstract class ExternalTransportBase<TSettings, TEndpoint> : TransportBase where TSettings : ExternalTransportSettings<TEndpoint> where TEndpoint : class
    {
        private readonly TSettings _settings;

        public ExternalTransportBase(string protocol, TSettings settings, IDurableMessagingFactory factory, ITransportLogger logger, JasperOptions jasperOptions)
            : base(protocol, factory, logger, jasperOptions)
        {
            _settings = settings;
        }

        protected sealed override ISender createSender(Uri uri, CancellationToken cancellation)
        {
            var transportUri = new TransportUri(uri);
            var endpoint = _settings.For(transportUri.ConnectionName);
            
            if (endpoint == null) throw new ArgumentOutOfRangeException(nameof(uri), $"Unknown {Protocol} connection named '{transportUri.ConnectionName}'");


            return buildSender(transportUri, endpoint, cancellation);
        }

        protected abstract ISender buildSender(TransportUri transportUri, TEndpoint endpoint,
            CancellationToken cancellation);
        
        protected override IListeningAgent buildListeningAgent(Uri uri, JasperOptions settings)
        {
            var transportUri = new TransportUri(uri);
            var endpoint = _settings.For(transportUri.ConnectionName);
            
            if (endpoint == null) throw new ArgumentOutOfRangeException(nameof(uri), $"Unknown {Protocol} connection named '{transportUri.ConnectionName}'");

            return buildListeningAgent(transportUri, endpoint, settings);
        }

        protected abstract IListeningAgent buildListeningAgent(TransportUri transportUri, TEndpoint endpoint, JasperOptions settings);

        protected override Uri[] validateAndChooseReplyChannel(Uri[] incoming)
        {
            var replies = _settings.For(ReplyUri.Host);
            if (replies != null)
            {
                return incoming.Concat(new Uri[] {ReplyUri}).Distinct().ToArray();
            }

            return incoming;
        }


    }
}