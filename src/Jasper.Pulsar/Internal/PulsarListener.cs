using System;
using System.Threading;
using System.Threading.Tasks;
using DotPulsar;
using DotPulsar.Abstractions;
using Jasper.Logging;
using Jasper.Runtime;
using Jasper.Transports;

namespace Jasper.Pulsar.Internal
{
    public class PulsarListener : IListener
    {
        private readonly CancellationToken _cancellation;
        private readonly IConsumer _consumer;
        private readonly PulsarEndpoint _endpoint;
        private readonly ITransportLogger _logger;
        private IListeningWorkerQueue _callback;
        private readonly ITransportProtocol<PulsarMessage> _protocol;
        private Task _consumerTask;

        public PulsarListener(PulsarEndpoint endpoint, ITransportLogger logger, CancellationToken cancellation)
        {
            _endpoint = endpoint;
            _logger = logger;
            _cancellation = cancellation;
            _protocol = new PulsarTransportProtocol();
            _consumer = endpoint.PulsarClient.CreateConsumer(endpoint.ConsumerOptions);
        }

        public void Dispose()
        {
            _consumer?.DisposeAsync();
            _consumerTask?.Dispose();
        }

        public Uri Address => _endpoint.Uri;
        public ListeningStatus Status { get; set; }

        public void Start(IListeningWorkerQueue callback)
        {
            _callback = callback;

            _consumerTask = ConsumeAsync();

            _logger.ListeningStatusChange(ListeningStatus.Accepting);
        }

        public void StartHandlingInline(IHandlerPipeline pipeline)
        {
            throw new NotImplementedException();
        }

        private async Task ConsumeAsync()
        {
            await foreach (Message message in _consumer.Messages(_cancellation))
            {
                Envelope envelope;

                try
                {
                    envelope = _protocol.ReadEnvelope(new PulsarMessage(message.Data, message.Properties));
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex, message: $"Error trying to map an incoming Pulsar {_endpoint.Topic} Topic message to an Envelope. See the Dead Letter Queue");
                    continue;
                }

                try
                {
                    await _callback.Received(Address, envelope);

                    await _consumer.Acknowledge(message, _cancellation);
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
