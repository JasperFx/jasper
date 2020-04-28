using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Logging;
using Jasper.Transports.Sending;
using LamarCodeGeneration.Util;

namespace Jasper.ConfluentKafka.Internal
{
    public class ConfluentKafkaSender : ISender
    {
        private Dictionary<Type, KafkaPublisher> _publishers = new Dictionary<Type, KafkaPublisher>();
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
        }

        public void Dispose()
        {
            foreach (KafkaPublisher publisher in _publishers.Values)
            {
                publisher.Dispose();
            }
        }

        public Uri Destination => _endpoint.Uri;
        public int QueuedCount { get; }
        public bool Latched { get; private set; }
        public void Start(ISenderCallback callback)
        {
            _callback = callback;

            _sending = new ActionBlock<Envelope>(sendBySession, _endpoint.ExecutionOptions);
        }

        public Task Enqueue(Envelope envelope)
        {
            _sending.Post(envelope);

            return Task.CompletedTask;
        }

        public Task LatchAndDrain()
        {
            Latched = true;

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

        private async Task sendBySession(Envelope envelope)
        {
            try
            {
                Type messageType = envelope.Message.GetType();
                if (!_publishers.ContainsKey(messageType))
                {
                    KafkaPublisher publisher = typeof(KafkaPublisher<,>).CloseAndBuildAs<KafkaPublisher>(_endpoint.ProducerConfig, typeof(string), messageType);
                    _publishers.Add(messageType, publisher);
                }

                await _publishers[messageType].SendAsync(_endpoint.TopicName, envelope, CancellationToken.None);

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

        public async Task<bool> Ping(CancellationToken cancellationToken)
        {
            Envelope envelope = Envelope.ForPing(Destination);
            KafkaPublisher publisher = new KafkaPublisher<string, Ping>(_endpoint.ProducerConfig);
            await publisher.SendAsync("jasper-ping", envelope, cancellationToken);
            return true;
        }

        public bool SupportsNativeScheduledSend { get; }

    }

}
