using System;
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
        private IReceiverCallback _callback;
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
            _consumerTask?.Dispose();
        }

        public Uri Address => _endpoint.Uri;
        public ListeningStatus Status { get; set; }

        public void Start(IReceiverCallback callback)
        {
            _callback = callback;

            _consumer.Subscribe(new []{ _endpoint.TopicName });

            _consumerTask = ConsumeAsync();

            Thread.Sleep(1000); // let the consumer start consuming
        }

        private async Task ConsumeAsync()
        {
            while (!_cancellation.IsCancellationRequested)
            {
                ConsumeResult<byte[], byte[]> message;
                try
                {
                    message = await Task.Run(() => _consumer.Consume(), _cancellation);
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex, message: $"Error consuming message from Kafka topic {_endpoint.TopicName}");
                    return;
                }

                Envelope envelope;

                try
                {
                    envelope = _protocol.ReadEnvelope(message.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex, message: $"Error trying to map an incoming Kafka {_endpoint.TopicName} Topic message to an Envelope. See the Dead Letter Queue");
                    return;
                }

                try
                {
                    await _callback.Received(Address, new[] { envelope });

                    _consumer.Commit();
                }
                catch (KafkaException ke)
                {
                    if (ke.Error?.Code == ErrorCode.Local_NoOffset)
                    {
                        return;
                    }
                    _logger.LogException(ke, envelope.Id, "Error trying to receive a message from " + Address);
                }
                catch (Exception e)
                {
                    _logger.LogException(e, envelope.Id, "Error trying to receive a message from " + Address);
                }
            }
        }
    }
}
