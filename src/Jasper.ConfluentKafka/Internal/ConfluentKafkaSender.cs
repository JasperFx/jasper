using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Jasper.ConfluentKafka.Exceptions;
using Jasper.ConfluentKafka.Serialization;
using Jasper.Logging;
using Jasper.Transports;
using Jasper.Transports.Sending;

namespace Jasper.ConfluentKafka.Internal
{
    public class ConfluentKafkaSender<TKey, TVal> : ISender
    {
        private readonly ITransportProtocol<Message<TKey, TVal>> _protocol;
        private readonly KafkaEndpoint<TKey, TVal> _endpoint;
        private readonly ITransportLogger _logger;
        private readonly CancellationToken _cancellation;
        private ActionBlock<Envelope> _sending;
        private ISenderCallback _callback;
        private IProducer<TKey, TVal> _publisher;

        private readonly ISerializer<TKey> _keySerializer = new DefaultJsonSerializer<TKey>().AsSyncOverAsync();
        private readonly ISerializer<TVal> _valueSerializer = new DefaultJsonSerializer<TVal>().AsSyncOverAsync();

        public ConfluentKafkaSender(KafkaEndpoint<TKey, TVal> endpoint, ITransportLogger logger, ISerializer<TKey> keySerializer, ISerializer<TVal> valueSerializer, CancellationToken cancellation)
        {
            _endpoint = endpoint;
            _logger = logger;
            _cancellation = cancellation;
            Destination = endpoint.Uri;
            _protocol = new KafkaTransportProtocol<TKey, TVal>();
            _keySerializer = keySerializer;
            _valueSerializer = valueSerializer;
        }

        public void Dispose()
        {
            _publisher?.Dispose();
        }

        public Uri Destination { get; }
        public int QueuedCount => _sending.InputCount;
        public bool Latched { get; private set; }


        public void Start(ISenderCallback callback)
        {
            _callback = callback;

            var publisherBuilder = new ProducerBuilder<TKey, TVal>(_endpoint.ProducerConfig)
                .SetKeySerializer(_keySerializer)
                .SetValueSerializer(_valueSerializer);
            
            _publisher = publisherBuilder.Build();

            _sending = new ActionBlock<Envelope>(sendBySession, new ExecutionDataflowBlockOptions
            {
                CancellationToken = _cancellation
            });
        }


        public Task Enqueue(Envelope envelope)
        {
            _sending.Post(envelope);

            return Task.CompletedTask;
        }

        public async Task LatchAndDrain()
        {
            Latched = true;

            _publisher.Flush(_cancellation);

            _sending.Complete();

            _logger.CircuitBroken(Destination);
        }

        public void Unlatch()
        {
            _logger.CircuitResumed(Destination);

            Start(_callback);
            Latched = false;
        }

        public async Task<bool> Ping(CancellationToken cancellationToken)
        {
            Envelope envelope = Envelope.ForPing(Destination);
            Message<TKey, TVal> message = _protocol.WriteFromEnvelope(envelope);

            message.Headers.Add("MessageGroupId", Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
            message.Headers.Add("Jasper_SessionId", Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));

            await _publisher.ProduceAsync("jasper-ping", message, cancellationToken);

            return true;
        }

        public bool SupportsNativeScheduledSend { get; } = false;

        private async Task sendBySession(Envelope envelope)
        {
            try
            {
                Message<TKey, TVal> message = _protocol.WriteFromEnvelope(envelope);
                message.Headers.Add("Jasper_SessionId",  Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));

                if (envelope.IsDelayed(DateTime.UtcNow))
                {
                    throw new UnsupportedFeatureException("Delayed Message Delivery");
                }
                else
                {
                    await _publisher.ProduceAsync(_endpoint.TopicName, message, _cancellation);
                }

                await _callback.Successful(envelope);
            }
            catch (Exception e)
            {
                try
                {
                    await _callback.ProcessingFailure(envelope, e);
                }
                catch (Exception exception)
                {
                    _logger.LogException(exception);
                }
            }
        }
    }
}
