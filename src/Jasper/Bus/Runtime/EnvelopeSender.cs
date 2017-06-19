using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Bus.Runtime.Subscriptions;

namespace Jasper.Bus.Runtime
{
    public class EnvelopeSender : IEnvelopeSender
    {
        private readonly ChannelGraph _channels;
        private readonly IEnvelopeSerializer _serializer;
        private readonly ISubscriptionsStorage _subscriptions;

        public EnvelopeSender(ChannelGraph channels, IEnvelopeSerializer serializer, ISubscriptionsStorage subscriptions, IBusLogger[] loggers)
        {
            _channels = channels;
            _serializer = serializer;
            _subscriptions = subscriptions;
            Logger = BusLogger.Combine(loggers);
        }

        public IBusLogger Logger { get; }

        public async Task<string> Send(Envelope envelope)
        {
            var channels = DetermineDestinationChannels(envelope).Distinct().ToArray();
            if (!channels.Any())
            {
                throw new Exception($"No channels match this message ({envelope})");
            }

            foreach (var channel in channels)
            {
                var sent = await _channels.Send(envelope, channel, _serializer).ConfigureAwait(false);
                Logger.Sent(sent);
            }

            return envelope.CorrelationId;
        }

        public async Task<string> Send(Envelope envelope, IMessageCallback callback)
        {
            var channels = DetermineDestinationChannels(envelope).ToArray();
            if (!channels.Any())
            {
                throw new Exception($"No channels match this message ({envelope})");
            }

            foreach (var channel in channels)
            {
                var sent = await _channels.Send(envelope, channel, _serializer, callback).ConfigureAwait(false);
                Logger.Sent(sent);
            }

            return envelope.CorrelationId;
        }

        // TODO -- have this return the channels maybe
        public IEnumerable<Uri> DetermineDestinationChannels(Envelope envelope)
        {
            var destination = envelope.Destination;

            if (destination != null)
            {
                yield return destination;

                yield break;
            }

            if (envelope.Message != null)
            {
                var messageType = envelope.Message.GetType();
                foreach (var channel in _channels)
                {
                    // TODO -- maybe memoize this one later
                    if (channel.ShouldSendMessage(messageType))
                    {
                        // TODO -- hang on here, should this be the "corrected" Uri
                        yield return channel.Uri;
                    }
                }

                foreach (var sub in _subscriptions.GetSubscribersFor(messageType))
                {
                    yield return sub;
                }
            }
        }
    }
}
