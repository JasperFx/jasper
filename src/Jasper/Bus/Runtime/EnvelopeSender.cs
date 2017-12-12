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
        private readonly BusSettings _settings;


        public EnvelopeSender(CompositeLogger logger, IMessageRouter router, IChannelGraph channels, BusSettings settings)
        {
            _router = router;
            _channels = channels;
            _settings = settings;

            Logger = logger;
        }

        public IBusLogger Logger { get;}

        public async Task<Guid> Send(Envelope envelope)
        {
            if (envelope.Message == null) throw new ArgumentNullException(nameof(envelope.Message));

            var outgoing = await _router.Route(envelope);

            if (!outgoing.Any())
            {
                Logger.NoRoutesFor(envelope);

                if (_settings.NoMessageRouteBehavior == NoRouteBehavior.ThrowOnNoRoutes)
                {
                    throw new NoRoutesException(envelope);
                }
            }

            foreach (var outgoingEnvelope in outgoing)
            {
                await outgoingEnvelope.Send();
            }

            return envelope.Id;
        }
    }
}
