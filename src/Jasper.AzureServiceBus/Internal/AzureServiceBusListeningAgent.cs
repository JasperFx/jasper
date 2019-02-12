using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Receiving;
using Microsoft.Azure.ServiceBus;

namespace Jasper.AzureServiceBus.Internal
{
    public class AzureServiceBusListeningAgent : IListeningAgent
    {
        private readonly IAzureServiceBusProtocol _protocol;
        private readonly ITransportLogger _logger;
        private readonly IQueueClient _client;
        private IReceiverCallback _callback;

        public AzureServiceBusListeningAgent(AzureServiceBusEndpoint endpoint, ITransportLogger logger, CancellationToken cancellation)
        {
            _logger = logger;

            _client = endpoint.TokenProvider != null
                ? new QueueClient(endpoint.ConnectionString, endpoint.Uri.QueueName, endpoint.TokenProvider, endpoint.TransportType, endpoint.ReceiveMode, endpoint.RetryPolicy)
                : new QueueClient(endpoint.ConnectionString, endpoint.Uri.QueueName, endpoint.ReceiveMode, endpoint.RetryPolicy);


            _protocol = endpoint.Protocol;
            Address = endpoint.Uri.ToUri();
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
                envelope = _protocol.ReadEnvelope(message);
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
