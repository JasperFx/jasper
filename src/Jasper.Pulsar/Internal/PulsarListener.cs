using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotPulsar;
using DotPulsar.Abstractions;
using Jasper.Logging;
using Jasper.Transports;

namespace Jasper.Pulsar.Internal
{
    public class PulsarListener : IListener
    {
        private readonly CancellationToken _cancellation;
        private readonly IConsumer _consumer;
        private readonly PulsarEndpoint _endpoint;
        private readonly ITransportLogger _logger;
        private readonly ITransportProtocol<PulsarMessage> _protocol;

        public PulsarListener(PulsarEndpoint endpoint, ITransportLogger logger, CancellationToken cancellation)
        {
            _endpoint = endpoint;
            _logger = logger;
            _cancellation = cancellation;
            _protocol = new PulsarTransportProtocol();
            _consumer = endpoint.PulsarClient.CreateConsumer(endpoint.ConsumerOptions);
        }

        public Uri Address => _endpoint.Uri;
        public ListeningStatus Status { get; set; }

        public async IAsyncEnumerable<(Envelope Envelope, object AckObject)> Consume()
        {
            _logger.ListeningStatusChange(ListeningStatus.Accepting);

            await foreach (Message message in _consumer.Messages(_cancellation))
            {
                Envelope envelope;

                try
                {
                    envelope = _protocol.ReadEnvelope(new PulsarMessage(message.Data, message.Properties));
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex, message: $"Error trying to map an incoming Pulsar {_endpoint.Topic} Topic message to an Envelope");
                    continue;
                }

                yield return (envelope, message.MessageId);
            }
        }


        public Task Ack((Envelope Envelope, object AckObject) messageInfo)
        {
            var ackObj = (MessageId)messageInfo.AckObject;

            return _consumer.Acknowledge(ackObj, _cancellation).AsTask();
        }

        public Task Nack((Envelope Envelope, object AckObject) messageInfo) => Task.CompletedTask;

        public void Dispose()
        {
            _consumer.DisposeAsync();
        }
    }
}
