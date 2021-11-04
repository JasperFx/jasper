using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using DotPulsar;
using DotPulsar.Abstractions;
using DotPulsar.Extensions;
using Jasper.Transports.Sending;

namespace Jasper.Pulsar
{
    public class PulsarSender : ISender
    {
        private readonly PulsarEndpoint _endpoint;
        private readonly PulsarTransport _transport;
        private readonly CancellationToken _cancellation;
        private readonly IProducer<ReadOnlySequence<byte>> _producer;

        public PulsarSender(PulsarEndpoint endpoint, PulsarTransport transport, CancellationToken cancellation)
        {
            _endpoint = endpoint;
            _transport = transport;
            _cancellation = cancellation;

            // TODO -- make this more configurable with ConsumerOptions
            _producer = transport.Client.NewProducer().Topic(_endpoint.PulsarTopic()).Create();

            Destination = _endpoint.Uri;
        }

        public void Dispose()
        {
            // TODO -- don't mix!
            _producer.DisposeAsync().GetAwaiter().GetResult();
        }

        public bool SupportsNativeScheduledSend { get; } = true;
        public Uri Destination { get; }
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

        // TODO -- use a ValueTask here
        public Task Send(Envelope envelope)
        {
            var message = _endpoint.Protocol.WriteFromEnvelope(envelope);

            return _producer.Send(message, new ReadOnlySequence<byte>(envelope.Data), _cancellation).AsTask();
        }
    }
}
