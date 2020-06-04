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
        private IListeningWorkerQueue _callback;
        private IHandlerPipeline _pipeline;
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
            _callback = callback;

            _consumer.Subscribe(new []{ _endpoint.TopicName });

            _consumerTask = ConsumeAsync();

            _logger.ListeningStatusChange(ListeningStatus.Accepting);
        }

        public void StartHandlingInline(IHandlerPipeline pipeline)
        {
            _pipeline = pipeline;

            _consumer.Subscribe(new[] { _endpoint.TopicName });

            _consumerTask = ConsumeInlineAsync();

            _logger.ListeningStatusChange(ListeningStatus.Accepting);
        }

        private async Task ConsumeAsync()
        {
            while (!_cancellation.IsCancellationRequested)
            {
                ConsumeResult<byte[], byte[]> result = await NextMessageAsync();

                if (result == null)
                    continue;

                Envelope envelope = DeserializeResult(result);

                if (envelope == null)
                    continue;

                try
                {
                    await _callback.Received(Address, envelope);
                }
                catch (Exception e)
                {
                    _logger.LogException(e, envelope.Id, "Error trying to receive a message from " + Address);
                }
            }
        }

        private async Task ConsumeInlineAsync()
        {
            while (!_cancellation.IsCancellationRequested)
            {
                ConsumeResult<byte[], byte[]> result = await NextMessageAsync();

                if(result == null)
                    continue;

                Envelope envelope = DeserializeResult(result);

                if(envelope == null)
                    continue;

                try
                {
                    await _pipeline.Invoke(envelope, new KafkaChannelCallback(result, _consumer));
                }
                catch (Exception e)
                {
                    _logger.LogException(e, envelope.Id, "Error trying to process message inline from " + Address);
                }
            }
        }


        Envelope DeserializeResult(ConsumeResult<byte[], byte[]> result)
        {
            try
            {
                return _protocol.ReadEnvelope(result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, message: $"Error trying to map an incoming Kafka {_endpoint.TopicName} Topic message to an Envelope. See the Dead Letter Queue");
                return null;
            }
        }

        async Task<ConsumeResult<byte[], byte[]>> NextMessageAsync()
        {
            try
            {
                ConsumeResult<byte[], byte[]> next = await Task.Run(() => _consumer.Consume(), _cancellation);
                return next;
            }
            catch (Confluent.Kafka.ConsumeException cex)
            {
                if (cex.Error.Code == ErrorCode.PolicyViolation)
                {
                    throw;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, message: $"Error consuming message from Kafka topic {_endpoint.TopicName}");
                return null;
            }
        }
    }
}
