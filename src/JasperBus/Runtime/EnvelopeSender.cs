using System;
using System.Collections.Generic;
using System.Linq;
using JasperBus.Configuration;
using JasperBus.Runtime.Serializers;
using System.Threading.Tasks;

namespace JasperBus.Runtime
{
    public class EnvelopeSender : IEnvelopeSender
    {
        private readonly ChannelGraph _channels;
        private readonly IEnvelopeSerializer _serializer;

        public EnvelopeSender(ChannelGraph channels, IEnvelopeSerializer serializer, IBusLogger[] loggers)
        {
            _channels = channels;
            _serializer = serializer;
            Logger = BusLogger.Combine(loggers);
        }

        public IBusLogger Logger { get; }

        public async Task<string> Send(Envelope envelope)
        {
            var channels = DetermineDestinationChannels(envelope).ToArray();
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
            }
        }




    }
}