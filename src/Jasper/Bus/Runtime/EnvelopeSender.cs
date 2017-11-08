using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime.Routing;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;

namespace Jasper.Bus.Runtime
{
    public class EnvelopeSender : IEnvelopeSender
    {
        private readonly IMessageRouter _router;
        private readonly IChannelGraph _channels;
        private readonly UriAliasLookup _aliases;
        private readonly BusSettings _settings;


        public EnvelopeSender(CompositeLogger logger, IMessageRouter router, IChannelGraph channels, UriAliasLookup aliases, BusSettings settings)
        {
            _router = router;
            _channels = channels;
            _aliases = aliases;
            _settings = settings;

            Logger = logger;
        }

        public IBusLogger Logger { get;}

        public async Task<string> Send(Envelope envelope)
        {
            if (envelope.Message == null) throw new ArgumentNullException(nameof(envelope.Message));

            envelope.Source = _settings.NodeId;

            if (envelope.Destination == null)
            {
                var routes = await _router.Route(envelope.Message.GetType());
                if (!routes.Any())
                {
                    Logger.NoRoutesFor(envelope);

                    if (_settings.NoMessageRouteBehavior == NoRouteBehavior.ThrowOnNoRoutes)
                    {
                        throw new NoRoutesException(envelope);
                    }
                }

                foreach (var route in routes)
                {
                    await sendEnvelope(envelope, route);
                }
            }
            else
            {
                var route = await _router.RouteForDestination(envelope);
                await sendEnvelope(envelope, route);
            }

            return envelope.Id;
        }

        public Task EnqueueLocally(object message)
        {
            var channel = _channels.DefaultChannel;
            var envelope = new Envelope
            {
                Message = message,
                Destination = channel.Uri
            };

            return Send(envelope);
        }

        private async Task<Envelope> sendEnvelope(Envelope envelope, MessageRoute route)
        {
            if (route == null) throw new ArgumentNullException(nameof(route));


            if (!envelope.RequiresLocalReply)
            {
                envelope.ReplyUri = envelope.ReplyUri ?? _channels.SystemReplyUri;
            }

            var sending = await route.Send(envelope);
            Logger.Sent(sending);

            return sending;
        }
    }
}
