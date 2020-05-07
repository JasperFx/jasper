using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Jasper.ConfluentKafka.Exceptions;
using Jasper.Logging;
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
            if(endpoint?.ProducerConfig == null)
                throw new ArgumentNullException(nameof(KafkaEndpoint.ProducerConfig));

            _endpoint = endpoint;
            _publisher = new ProducerBuilder<byte[], byte[]>(endpoint.ProducerConfig)
                .SetErrorHandler((producer, error) =>
                {
                    if (error.IsFatal)
                    {
                        throw new KafkaSenderException(error);
                    }
                })
                .Build();
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
                await Send(envelope);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public Task Send(Envelope envelope)
        {
            if (envelope.IsDelayed(DateTime.UtcNow))
            {
                throw new UnsupportedFeatureException("Delayed Message Delivery");
            }

            Message<byte[], byte[]> message = _protocol.WriteFromEnvelope(envelope);
            
            return _publisher.ProduceAsync(_endpoint.TopicName, message);
        }
    }
}
