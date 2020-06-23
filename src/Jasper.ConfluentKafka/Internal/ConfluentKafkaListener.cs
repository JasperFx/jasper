using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Jasper.ConfluentKafka;
using Jasper.ConfluentKafka.Internal;
using Jasper.Logging;
using Jasper.Runtime;
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

        public Uri Address => _endpoint.Uri;
        public ListeningStatus Status { get; set; }

        public void Start(IListeningWorkerQueue callback)
        {
            _consumer.Subscribe(new []{ _endpoint.TopicName });

            _consumerTask = ConsumeAsync(callback);

            _logger.ListeningStatusChange(ListeningStatus.Accepting);
        }

        public void StartHandlingInline(IHandlerPipeline pipeline)
        {
            _consumer.Subscribe(new[] { _endpoint.TopicName });

            _consumerTask = ConsumeAsync(pipeline);

            _logger.ListeningStatusChange(ListeningStatus.Accepting);
        }

        private async Task ConsumeAsync(IListeningWorkerQueue callback)
        {
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

                try
                {
                    await callback.Received(Address, envelope);

                    _consumer.Commit(message);
                }
                catch (Exception e)
                {
                    // TODO -- Got to either discard this or defer it back to the queue
                    _logger.LogException(e, envelope.Id, "Error trying to receive a message from " + Address);
                }
            }
        }

        private async Task ConsumeAsync(IHandlerPipeline pipeline)
        {
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

                try
                {
                    await pipeline.Invoke(envelope, new KafkaChannelCallback(message, _consumer));
                }
                catch (Exception e)
                {
                    // TODO -- Got to either discard this or defer it back to the queue
                    _logger.LogException(e, envelope.Id, "Error trying to receive a message from " + Address);
                }
            }
        }
    }
}
