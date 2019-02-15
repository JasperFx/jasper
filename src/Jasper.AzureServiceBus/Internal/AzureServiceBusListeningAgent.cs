using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
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
        private readonly AzureServiceBusEndpoint _endpoint;
        private readonly ITransportLogger _logger;
        private readonly CancellationToken _cancellation;
        private IReceiverCallback _callback;
        private IClientEntity _listeningClient;

        public AzureServiceBusListeningAgent(AzureServiceBusEndpoint endpoint, ITransportLogger logger, CancellationToken cancellation)
        {
            _endpoint = endpoint;
            _logger = logger;
            _cancellation = cancellation;


            _protocol = endpoint.Protocol;
            Address = endpoint.Uri.ToUri();

        }


        public void Dispose()
        {
            _listeningClient.CloseAsync().GetAwaiter().GetResult();
        }

        public Uri Address { get; }
        public ListeningStatus Status { get; set; }

        public void Start(IReceiverCallback callback)
        {
            _callback = callback;

            var options = new SessionHandlerOptions(handleException);

            var connectionString = _endpoint.ConnectionString;
            var tokenProvider = _endpoint.TokenProvider;
            var subscriptionName = _endpoint.Uri.SubscriptionName;
            var queueName = _endpoint.Uri.QueueName;
            var retryPolicy = _endpoint.RetryPolicy;
            var receiveMode = _endpoint.ReceiveMode;
            var transportType = _endpoint.TransportType;

            if (_endpoint.Uri.TopicName.IsEmpty())
            {


                var client = tokenProvider != null
                    ? new QueueClient(connectionString, queueName, tokenProvider, transportType, receiveMode, retryPolicy)
                    : new QueueClient(connectionString, queueName, receiveMode, retryPolicy);

                client.RegisterSessionHandler(handleMessage, options);

                _listeningClient = client;
            }
            else if (_endpoint.Uri.IsMessageSpecificTopic())
            {

                throw new NotImplementedException();

            }
            else
            {
                var client = tokenProvider != null
                    ? new SubscriptionClient(connectionString, _endpoint.Uri.TopicName, subscriptionName, tokenProvider,
                        transportType, receiveMode, retryPolicy)
                    : new SubscriptionClient(connectionString, _endpoint.Uri.TopicName, subscriptionName,
                        receiveMode, retryPolicy);

                client.RegisterSessionHandler(handleMessage, options);

                _listeningClient = client;
            }


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
