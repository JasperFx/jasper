using System.Threading.Tasks;
using Confluent.Kafka;
using Jasper.Transports;

namespace Jasper.ConfluentKafka.Internal
{
    class KafkaChannelCallback : IChannelCallback
    {
        private readonly ConsumeResult<byte[], byte[]> _consumerResult;
        private readonly IConsumer<byte[], byte[]> _consumer;

        public KafkaChannelCallback(ConsumeResult<byte[], byte[]> consumerResult, IConsumer<byte[], byte[]> consumer)
        {
            _consumerResult = consumerResult;
            _consumer = consumer;
        }

        public Task Complete(Envelope envelope)
        {
            _consumer.Commit(_consumerResult);
            return Task.CompletedTask;
        }

        public Task Defer(Envelope envelope) => Task.CompletedTask;
    }
}
