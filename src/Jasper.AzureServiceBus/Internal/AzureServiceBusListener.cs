using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Logging;
using Jasper.Runtime;
using Jasper.Transports;
using Microsoft.Azure.ServiceBus;

namespace Jasper.AzureServiceBus.Internal
{
    public class AzureServiceBusListener : IListener
    {
        private readonly CancellationToken _cancellation;

        private readonly IList<IClientEntity> _clientEntities = new List<IClientEntity>();
        private readonly AzureServiceBusEndpoint _endpoint;
        private readonly ITransportLogger _logger;
        private readonly ITransportProtocol<Message> _protocol;
        private readonly AzureServiceBusTransport _transport;
        private IListeningWorkerQueue _callback;


        public AzureServiceBusListener(AzureServiceBusEndpoint endpoint, AzureServiceBusTransport transport,
            ITransportLogger logger, CancellationToken cancellation)
        {
            _endpoint = endpoint;
            _transport = transport;
            _logger = logger;
            _cancellation = cancellation;


            _protocol = endpoint.Protocol;
            Address = endpoint.Uri;
        }


        public void Dispose()
        {
            foreach (var entity in _clientEntities) entity.CloseAsync().GetAwaiter().GetResult();
        }

        public Uri Address { get; }
        public ListeningStatus Status { get; set; }

        public void Start(IListeningWorkerQueue callback)
        {
            _callback = callback;

            var options = new SessionHandlerOptions(handleException);

            var connectionString = _transport.ConnectionString;
            var tokenProvider = _transport.TokenProvider;
            var subscriptionName = _endpoint.SubscriptionName;
            var queueName = _endpoint.QueueName;
            var retryPolicy = _transport.RetryPolicy;
            var receiveMode = _transport.ReceiveMode;
            var transportType = _transport.TransportType;
            var topicName = _endpoint.TopicName;

            if (topicName.IsEmpty())
            {
                var client = tokenProvider != null
                    ? new QueueClient(connectionString, queueName, tokenProvider, transportType, receiveMode,
                        retryPolicy)
                    : new QueueClient(connectionString, queueName, receiveMode, retryPolicy);

                client.RegisterSessionHandler(handleMessage, options);

                _clientEntities.Add(client);
            }
            else
            {
                var client = tokenProvider != null
                    ? new SubscriptionClient(connectionString, topicName, subscriptionName, tokenProvider,
                        transportType, receiveMode, retryPolicy)
                    : new SubscriptionClient(connectionString, topicName, subscriptionName,
                        receiveMode, retryPolicy);

                client.RegisterSessionHandler(handleMessage, options);

                _clientEntities.Add(client);
            }
        }

        public void StartHandlingInline(IHandlerPipeline pipeline)
        {
            throw new NotImplementedException();
        }

        private Task handleException(ExceptionReceivedEventArgs arg)
        {
            _logger.LogException(arg.Exception,
                message: "Internal failure in the Azure Service Bus QueueClient for " + Address);
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
                _logger.LogException(e,
                    message:
                    "Error trying to map an incoming Azure Service Bus message to an Envelope. See the Dead Letter Queue");
                await session.DeadLetterAsync(lockToken, "Bad Envelope", e.ToString());

                return;
            }

            try
            {
                await _callback.Received(Address, envelope);
                await session.CompleteAsync(lockToken);
            }
            catch (Exception e)
            {
                _logger.LogException(e, envelope.Id, "Error trying to receive a message from " + Address);
                await session.AbandonAsync(lockToken);
            }
        }
    }
}
