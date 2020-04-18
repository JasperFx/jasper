using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Jasper.ConfluentKafka;
using Jasper.ConfluentKafka.Serialization;
using Jasper.Logging;
using Jasper.Transports;
using Lamar.IoC.Instances;

namespace Jasper.Kafka.Internal
{
    public class ConfluentKafkaListener<TKey, TVal> : IListener
    {
        private readonly CancellationToken _cancellation;

        private readonly KafkaEndpoint _endpoint;
        private readonly ITransportLogger _logger;
        private readonly KafkaTransportProtocol<TKey, TVal> _protocol = new KafkaTransportProtocol<TKey, TVal>();
        private IReceiverCallback _callback;
        private IConsumer<TKey, TVal> _consumer;
        private readonly IDeserializer<TKey> _keyDeserializer = new DefaultJsonDeserializer<TKey>().AsSyncOverAsync();
        private readonly IDeserializer<TVal> _valueDeserializer = new DefaultJsonDeserializer<TVal>().AsSyncOverAsync();

        private Task _consumerTask;

        public ConfluentKafkaListener(KafkaEndpoint endpoint, ITransportLogger logger, IDeserializer<TKey> keyDeserializer, IDeserializer<TVal> valueDeserializer, CancellationToken cancellation)
        {
            _endpoint = endpoint;
            _logger = logger;
            _cancellation = cancellation;
            Address = endpoint.Uri;
            _keyDeserializer = keyDeserializer;
            _valueDeserializer = valueDeserializer;
        }


        public void Dispose()
        {
            _consumerTask?.Dispose();
            _consumer?.Dispose();
        }

        public Uri Address { get; }
        public ListeningStatus Status { get; set; }

        public void Start(IReceiverCallback callback)
        {
            _callback = callback;

            _consumer = new ConsumerBuilder<TKey, TVal>(_endpoint.ConsumerConfig)
                .SetKeyDeserializer(_keyDeserializer)
                .SetValueDeserializer(_valueDeserializer)
                .Build();

            _consumer.Subscribe(_endpoint.TopicName);

            _consumerTask = ConsumeAsync();

            Thread.Sleep(1000); // let the consumer start consuming
        }

        private async Task ConsumeAsync()
        {
            while (!_cancellation.IsCancellationRequested)
            {
                ConsumeResult<TKey, TVal> message;
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
                    await _callback.Received(Address, new[] {envelope});

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
