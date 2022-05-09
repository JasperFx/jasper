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
        private readonly CancellationToken _cancellation;
        private readonly IProducer<ReadOnlySequence<byte>> _producer;

        public PulsarSender(PulsarEndpoint endpoint, PulsarTransport transport, CancellationToken cancellation)
        {
            _endpoint = endpoint;
            _cancellation = cancellation;

            // TODO -- make this more configurable with ConsumerOptions
            _producer = transport.Client!.NewProducer().Topic(_endpoint.PulsarTopic()).Create();

            Destination = _endpoint.Uri;
        }

        public void Dispose()
        {
            // TODO -- don't mix! Use DisposeAsync() wherever possible
            // TODO -- might be a good Baseline function to Dispose or AsyncDispose an enumerable of objects
            _producer.DisposeAsync().GetAwaiter().GetResult();
        }

        public bool SupportsNativeScheduledSend { get; } = true;
        public Uri Destination { get; }
        public async Task<bool> PingAsync()
        {
            Envelope envelope = Envelope.ForPing(Destination);
            try
            {
                await SendAsync(envelope);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public async ValueTask SendAsync(Envelope envelope)
        {
            var message = new MessageMetadata();

            _endpoint.MapEnvelopeToOutgoing(envelope, message);

            await _producer.Send(message, new ReadOnlySequence<byte>(envelope.Data!), _cancellation);
        }
    }
}
