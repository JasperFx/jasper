using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Jasper.ConfluentKafka;
using Jasper.Logging;
using Jasper.Transports;

namespace Jasper.Kafka.Internal
{
    public class ConfluentKafkaListener : IListener
    {
        private readonly CancellationToken _cancellation;
        private readonly IConsumer<byte[], byte[]> _consumer;
        private readonly KafkaEndpoint _endpoint;
        private readonly ITransportLogger _logger;
        private readonly ITransportProtocol<Message<byte[], byte[]>> _protocol;
        private Task _consumerTask;

        public ConfluentKafkaListener(KafkaEndpoint endpoint, ITransportLogger logger, CancellationToken cancellation)
        {
            _endpoint = endpoint;
            _logger = logger;
            _cancellation = cancellation;
            _protocol = new KafkaTransportProtocol();
            _consumer = new ConsumerBuilder<byte[], byte[]>(endpoint.ConsumerConfig).Build();
        }

        public void Dispose()
        {
            _consumer?.Dispose();
            _consumerTask?.Dispose();
        }


        public Task Ack((Envelope Envelope, object AckObject) messageInfo)
        {
            var achObj = messageInfo.AckObject as ConsumeResult<byte[], byte[]>;

            _consumer.Commit(achObj);

            return Task.CompletedTask;
        }

        public Task Nack((Envelope Envelope, object AckObject) messageInfo) => Task.CompletedTask;

        public Uri Address => _endpoint.Uri; 
        public ListeningStatus Status { get; set; }

        public async IAsyncEnumerable<(Envelope Envelope, object AckObject)> Consume()
        {
            _consumer.Subscribe(new[] { _endpoint.TopicName });

            while (!_cancellation.IsCancellationRequested)
            {
                ConsumeResult<byte[], byte[]> message;
                try
                {
                    message = await Task.Run(() => _consumer.Consume(), _cancellation);
                }
                catch (Confluent.Kafka.ConsumeException cex)
                {
                    if (cex.Error.Code == ErrorCode.PolicyViolation)
                    {
                        throw;
                    }

                    continue; 
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex, message: $"Error consuming message from Kafka topic {_endpoint.TopicName}");
                    continue;
                }

                Envelope envelope;

                try
                {
                    envelope = _protocol.ReadEnvelope(message.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex, message: $"Error trying to map an incoming Kafka {_endpoint.TopicName} Topic message to an Envelope. See the Dead Letter Queue");
                    continue;
                }

                yield return (envelope, message);
            }
        }
    }
}
