using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Jasper.ConfluentKafka.Serialization;

namespace Jasper.ConfluentKafka.Internal
{
    public interface IKafkaConsumer
    {
        Task<(Envelope, TopicPartitionOffset)> ConsumeEnvelopeAsync(CancellationToken cancel);
        void Commit(TopicPartitionOffset topicPartitionOffset);
    }

    public abstract class KafkaConsumer : IKafkaConsumer, IDisposable
    {
        public abstract Task<(Envelope, TopicPartitionOffset)> ConsumeEnvelopeAsync(CancellationToken cancel);
        public abstract void Commit(TopicPartitionOffset offset);
        public abstract void Dispose();
    }

    public class KafkaConsumer<TKey, TVal> : KafkaConsumer
    {
        private readonly KafkaEndpoint _endpoint;
        private IConsumer<TKey, TVal> _consumer;
        private readonly IDeserializer<TKey> _keyDeserializer = new DefaultJsonDeserializer<TKey>().AsSyncOverAsync();
        private readonly IDeserializer<TVal> _valueDeserializer = new DefaultJsonDeserializer<TVal>().AsSyncOverAsync();

        public KafkaConsumer(KafkaEndpoint endpoint)
        {
            _endpoint = endpoint;
            _consumer = new ConsumerBuilder<TKey, TVal>(endpoint.ConsumerConfig)
                .SetKeyDeserializer(_keyDeserializer)
                .SetValueDeserializer(_valueDeserializer)
                .Build();

            _consumer.Subscribe(endpoint.TopicName);
        }

        public override async Task<(Envelope, TopicPartitionOffset)> ConsumeEnvelopeAsync(CancellationToken cancel)
        {
            ConsumeResult<TKey, TVal> message;
            try
            {
                message = await Task.Run(() => _consumer.Consume(), cancel);
            }
            catch (Exception ex)
            {
                throw new Exception("failed to consume messages");
            }

            Envelope envelope;  

            try
            {
                envelope = new KafkaTransportProtocol<TKey, TVal>().ReadEnvelope(message.Message);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error trying to map an incoming Kafka {_endpoint.TopicName} Topic message to an Envelope. See the Dead Letter Queue");
            }

            return (envelope, message.TopicPartitionOffset);
        }

        public override void Commit(TopicPartitionOffset offset)
        {
            _consumer.Commit(new []{offset });
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
