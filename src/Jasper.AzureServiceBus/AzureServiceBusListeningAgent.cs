using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Receiving;
using Microsoft.Azure.ServiceBus;

namespace Jasper.AzureServiceBus
{
    public class AzureServiceBusListeningAgent : IListeningAgent
    {
        private readonly IEnvelopeMapper _mapper;
        private readonly ITransportLogger _logger;
        private readonly IQueueClient _client;
        private IReceiverCallback _callback;

        public AzureServiceBusListeningAgent(AzureServiceBusSettings settings, IEnvelopeMapper mapper, Uri address, ITransportLogger logger)
        {
            _mapper = mapper;
            _logger = logger;
            Address = address;
            _client = settings.BuildClient(address);
        }

        public void Dispose()
        {
            _client.CloseAsync().GetAwaiter().GetResult();
        }

        public Uri Address { get; }
        public ListeningStatus Status { get; set; }

        public void Start(IReceiverCallback callback)
        {
            _callback = callback;

            var options = new SessionHandlerOptions(handleException);

            _client.RegisterSessionHandler(handleMessage, options);
        }

        private Task handleException(ExceptionReceivedEventArgs arg)
        {
            _logger.LogException(arg.Exception, message:"Internal failure in the Azure Service Bus QueueClient for " + Address);
            return Task.CompletedTask;
        }

        private async Task handleMessage(IMessageSession session, Message message, CancellationToken token)
        {
            var lockToken = message.SystemProperties.LockToken;

            Envelope envelope;

            try
            {
                envelope = _mapper.ReadEnvelope(message);
            }
            catch (Exception e)
            {
                _logger.LogException(e, message: "Error trying to map an incoming Azure Service Bus message to an Envelope. See the Dead Letter Queue");
                await session.DeadLetterAsync(lockToken, "Bad Envelope", e.ToString());

                return;
            }

            try
            {
                await _callback.Received(Address, new Envelope[] {envelope});
                await session.CompleteAsync(lockToken);
            }
            catch (Exception e)
            {
                _logger.LogException(e, envelope.Id, message:"Error trying to receive a message from " + Address);
                await session.AbandonAsync(lockToken);
            }
        }
    }
}
