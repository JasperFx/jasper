using System;
using System.Threading;
using System.Threading.Tasks;
using DotPulsar.Abstractions;
using Jasper.Transports;
using Jasper.Transports.Sending;

namespace Jasper.Pulsar.Internal
{
    public class PulsarSender : ISender
    {
        private readonly ITransportProtocol<PulsarMessage> _protocol;
        private readonly IProducer _publisher;
        private readonly PulsarEndpoint _endpoint;
        public bool SupportsNativeScheduledSend { get; } = false;
        public Uri Destination => _endpoint.Uri;
        public PulsarSender(PulsarEndpoint endpoint)
        {
            _endpoint = endpoint;
            _publisher = endpoint.PulsarClient.CreateProducer(endpoint.ProducerOptions);
            _protocol = new PulsarTransportProtocol();
        }

        public void Dispose()
        {
            _publisher?.DisposeAsync();
        }

        public async Task<bool> Ping(CancellationToken cancellationToken)
        {
            Envelope envelope = Envelope.ForPing(Destination);
            try
            {
                await Send(envelope);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public async Task Send(Envelope envelope)
        {
            if (envelope.IsDelayed(DateTime.UtcNow))
            {
                throw new Exception("Delayed Message Delivery");
            }

            var message = _protocol.WriteFromEnvelope(envelope);
            
            _ = await _publisher.Send(message.Metadata, message.Data);
        }
    }
}
