using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Configuration;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime.Routing;

namespace Jasper.Bus.Runtime
{
    public class EnvelopeSender : IEnvelopeSender
    {
        private readonly IMessageRouter _router;
        private readonly ChannelGraph _channels;
        private readonly IDictionary<string, ITransport> _transports = new Dictionary<string, ITransport>();


        public EnvelopeSender(IBusLogger[] loggers, IMessageRouter router, ChannelGraph channels, IEnumerable<ITransport> transports)
        {
            _router = router;
            _channels = channels;

            foreach (var transport in transports)
            {
                Baseline.DictionaryExtensions.SmartAdd(_transports, transport.Protocol, transport);
            }

            Logger = BusLogger.Combine(loggers);
        }

        public IBusLogger Logger { get;}

        public Task<string> Send(Envelope envelope)
        {
            return Send(envelope, null);
        }

        public async Task<string> Send(Envelope envelope, IMessageCallback callback)
        {
            if (envelope.Message == null) throw new ArgumentNullException(nameof(envelope.Message));



            if (envelope.Destination == null)
            {
                var routes = await _router.Route(envelope.Message.GetType());
                if (!routes.Any())
                {
                    throw new NoRoutesException(envelope);
                }

                foreach (var route in routes)
                {
                    await sendEnvelope(envelope, route, callback);
                }
            }
            else
            {
                var route = await _router.RouteForDestination(envelope);
                await sendEnvelope(envelope, route, callback);
            }

            return envelope.CorrelationId;
        }

        private async Task<Envelope> sendEnvelope(Envelope envelope, MessageRoute route, IMessageCallback callback)
        {
            if (route == null) throw new ArgumentNullException(nameof(route));

            var transportScheme = route.Destination.Scheme;
            if (_channels.HasChannel(route.Destination))
            {
                transportScheme = _channels[route.Destination].Uri.Scheme;
            }

            ITransport transport = null;
            if (_transports.TryGetValue(transportScheme, out transport))
            {
                var sending = route.CloneForSending(envelope);

                var channel = _channels.TryGetChannel(route.Destination);
                channel?.ApplyModifiers(sending);

                // Don't override the explicitly set Accepts
                if (!sending.AcceptedContentTypes.Any())
                {
                    sending.AcceptedContentTypes = _channels.AcceptedContentTypes.ToArray();
                }

                if (channel != null)
                {
                    await sendToStaticChannel(callback, sending, channel);
                }
                else
                {
                    await sendToDynamicChannel(route.Destination, callback, sending, transport);
                }

                Logger.Sent(sending);

                return sending;
            }
            else
            {
                throw new InvalidOperationException($"Unrecognized transport scheme '{transportScheme}'");
            }
        }

        private static async Task sendToDynamicChannel(Uri address, IMessageCallback callback, Envelope sending, ITransport transport)
        {
            sending.Destination = address;
            sending.ReplyUri = transport.DefaultReplyUri();

            if (callback == null)
            {
                await transport.Send(sending, sending.Destination).ConfigureAwait(false);
            }
            else
            {
                await callback.Send(sending).ConfigureAwait(false);
            }
        }

        private static async Task sendToStaticChannel(IMessageCallback callback, Envelope sending, ChannelNode channel)
        {
            sending.Destination = channel.Destination;
            sending.ReplyUri = channel.ReplyUri;

            if (callback == null || !callback.SupportsSend)
            {
                await channel.Sender.Send(sending).ConfigureAwait(false);
            }
            else
            {
                await callback.Send(sending).ConfigureAwait(false);
            }
        }

    }
}
