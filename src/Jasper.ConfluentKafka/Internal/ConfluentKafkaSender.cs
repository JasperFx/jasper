using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Jasper.ConfluentKafka.Exceptions;
using Jasper.Transports;
using Jasper.Transports.Sending;

namespace Jasper.ConfluentKafka.Internal
{
    public class ConfluentKafkaSender : ISender
    {
        private readonly ITransportProtocol<Message<byte[], byte[]>> _protocol;
        private readonly IProducer<byte[], byte[]> _publisher;
        private readonly KafkaEndpoint _endpoint;
        public bool SupportsNativeScheduledSend { get; } = false;
        public Uri Destination => _endpoint.Uri;
        public ConfluentKafkaSender(KafkaEndpoint endpoint)
        {
            _endpoint = endpoint;
            _publisher = new ProducerBuilder<byte[], byte[]>(endpoint.ProducerConfig).Build();
            _protocol = new KafkaTransportProtocol();
        }

        public void Dispose()
        {
            _publisher?.Dispose();
        }

        public async Task<bool> Ping(CancellationToken cancellationToken)
        {
            Envelope envelope = Envelope.ForPing(Destination);
            try
            {
                Message<byte[], byte[]> message = _protocol.WriteFromEnvelope(envelope);

                await _publisher.ProduceAsync("jasper-ping", message, cancellationToken);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public async Task Send(Envelope envelope)
        {
            if (envelope.IsDelayed(DateTime.UtcNow))
            {
                throw new UnsupportedFeatureException("Delayed Message Delivery");
            }

            Message<byte[], byte[]> message = _protocol.WriteFromEnvelope(envelope);
            try
            {
                var result = await _publisher.ProduceAsync(_endpoint.TopicName, message);
                Console.WriteLine(result.Status);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
