using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Confluent.Kafka;
using Jasper.ConfluentKafka.Exceptions;
using Jasper.Logging;
using Jasper.Transports;
using Jasper.Transports.Sending;

namespace Jasper.ConfluentKafka.Internal
{
    public class ConfluentKafkaSender : ISender
    {
        private ITransportProtocol<Message<byte[], byte[]>> _protocol;
        private IProducer<byte[], byte[]> _publisher;
        private readonly KafkaEndpoint _endpoint;
        private readonly ITransportLogger _logger;
        private readonly CancellationToken _cancellation;
        private ActionBlock<Envelope> _sending;
        private ISenderCallback _callback;

        public ConfluentKafkaSender(KafkaEndpoint endpoint, ITransportLogger logger, CancellationToken cancellation)
        {
            _endpoint = endpoint;
            _logger = logger;
            _cancellation = cancellation;
            _publisher = new ProducerBuilder<byte[], byte[]>(endpoint.ProducerConfig).Build();
            _sending = new ActionBlock<Envelope>(sendBySession, _endpoint.ExecutionOptions);
            _protocol = new KafkaTransportProtocol();
        }

        public void Dispose()
        {
            _publisher?.Dispose();
        }

        public Uri Destination => _endpoint.Uri;
        public int QueuedCount => _sending.InputCount;
        public bool Latched { get; private set; }

        public void Start(ISenderCallback callback)
        {
            _callback = callback;
        }

        public Task Send(Envelope envelope)
        {
            _sending.Post(envelope);

            return Task.CompletedTask;
        }

        public Task LatchAndDrain()
        {
            Latched = true;

            _publisher.Flush(_cancellation);

            _sending.Complete();

            _logger.CircuitBroken(Destination);

            return Task.CompletedTask;
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
            Message<byte[], byte[]> message = _protocol.WriteFromEnvelope(envelope);

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
                Message<byte[], byte[]> message = _protocol.WriteFromEnvelope(envelope);
                message.Headers.Add("Jasper_SessionId", Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));

                if (envelope.IsDelayed(DateTime.UtcNow))
                {
                    throw new UnsupportedFeatureException("Delayed Message Delivery");
                }

                await _publisher.ProduceAsync(_endpoint.TopicName, message, _cancellation);

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
