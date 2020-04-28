using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Jasper.ConfluentKafka;
using Jasper.ConfluentKafka.Internal;
using Jasper.ConfluentKafka.Serialization;
using Jasper.Logging;
using Jasper.Transports;
using Lamar.IoC.Instances;

namespace Jasper.Kafka.Internal
{
    public class ConfluentKafkaListener : IListener
    {
        private readonly CancellationToken _cancellation;
        private readonly KafkaConsumer<byte[], byte[]> _consumer;
        private readonly ITransportLogger _logger;
        private IReceiverCallback _callback;
        

        private Task _consumerTask;

        public ConfluentKafkaListener(KafkaEndpoint endpoint, ITransportLogger logger, CancellationToken cancellation)
        {
            _logger = logger;
            _cancellation = cancellation;
            Address = endpoint.Uri;
            _consumer= new KafkaConsumer<byte[], byte[]>(endpoint);
        }


        public void Dispose()
        {
            _consumerTask?.Dispose();
        }

        public Uri Address { get; }
        public ListeningStatus Status { get; set; }

        public void Start(IReceiverCallback callback)
        {
            _callback = callback;

            _consumerTask = ConsumeAsync();

            Thread.Sleep(1000); // let the consumer start consuming
        }

        private async Task ConsumeAsync()
        {
            while (!_cancellation.IsCancellationRequested)
            {
                Envelope envelope = null;
                try
                {
                    (Envelope Envelope, TopicPartitionOffset TopicPartitionOffset) receivedMessage = await _consumer.ConsumeEnvelopeAsync(_cancellation);
                    envelope = receivedMessage.Envelope;

                    await _callback.Received(Address, new[] { envelope });

                    _consumer.Commit(receivedMessage.TopicPartitionOffset);
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
