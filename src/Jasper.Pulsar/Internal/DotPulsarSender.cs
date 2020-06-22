using System;
using System.Threading;
using System.Threading.Tasks;
using DotPulsar.Abstractions;
using Jasper.Transports;
using Jasper.Transports.Sending;

namespace Jasper.DotPulsar.Internal
{
    public class PulsarSender : ISender
    {
        private readonly ITransportProtocol<DotPulsarMessage> _protocol;
        private readonly IProducer _publisher;
        private readonly DotPulsarEndpoint _endpoint;
        private readonly CancellationToken _cancellationToken;
        public bool SupportsNativeScheduledSend { get; } = true;
        public Uri Destination => _endpoint.Uri;
        public PulsarSender(DotPulsarEndpoint endpoint, CancellationToken cancellationToken)
        {
            _endpoint = endpoint;
            _cancellationToken = cancellationToken;
            _publisher = endpoint.PulsarClient.CreateProducer(endpoint.ProducerOptions);
            _protocol = new DotPulsarTransportProtocol();
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
            DotPulsarMessage message = _protocol.WriteFromEnvelope(envelope);
            
            _ = await _publisher.Send(message.Metadata, message.Data, _cancellationToken);
        }
    }
}
