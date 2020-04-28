using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Jasper.ConfluentKafka.Serialization;

namespace Jasper.ConfluentKafka.Internal
{
    public interface IKafkaPublisher
    {
        Task SendAsync(string topic, Envelope envelope, CancellationToken cancel);
    }
    public abstract class KafkaPublisher : IKafkaPublisher, IDisposable
    {
        public abstract Task SendAsync(string topic, Envelope envelope, CancellationToken cancel);

        public abstract void Dispose();
    }

    public class KafkaPublisher<TKey, TVal> : KafkaPublisher
    {
        private readonly IProducer<TKey, TVal> _producer;
        private readonly KafkaTransportProtocol<TKey, TVal> _protocol = new KafkaTransportProtocol<TKey, TVal>();
        // TODO: Move this logic somewhere else if we can. Can it be in Jasper's pipeline and we always expect byte[] here?
        private readonly ISerializer<TKey> _keySerializer = new DefaultJsonSerializer<TKey>().AsSyncOverAsync();
        private readonly ISerializer<TVal> _valueSerializer = new DefaultJsonSerializer<TVal>().AsSyncOverAsync();
        

        public KafkaPublisher(ProducerConfig producerConifg)
        {
            _producer = new ProducerBuilder<TKey, TVal>(producerConifg)
                .SetKeySerializer(_keySerializer)
                .SetValueSerializer(_valueSerializer)
                .Build();
        }

        public override Task SendAsync(string topic, Envelope envelope, CancellationToken cancel)
        {
            Message<TKey, TVal> message = _protocol.WriteFromEnvelope(envelope);
            return _producer.ProduceAsync(topic, message, cancel);
        }

        public override void Dispose()
        {
            _producer.Dispose();
        }
    }
}
