using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Baseline;
using Jasper.Logging;
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

        private readonly BufferBlock<(Envelope Envelope, object AckObject)> _buffer = new BufferBlock<(Envelope Envelope, object AckObject)>(new DataflowBlockOptions
        {// DO NOT CHANGE THESE SETTINGS THEY ARE IMPORTANT TO LINK RECEIVE DELEGATE WITH CONSUME()
            BoundedCapacity =  1,
            MaxMessagesPerTask = 1,
            EnsureOrdered = true
        });
        
        public async IAsyncEnumerable<(Envelope Envelope, object AckObject)> Consume()
        {
            Start();

            while(!_cancellation.IsCancellationRequested)
            {
                var received = await _buffer.ReceiveAsync(_cancellation);
                yield return received;
            }
        }

        public Task Ack((Envelope Envelope, object AckObject) messageInfo)
        {
            var ackObj = ((Task Ack, Task Nack))messageInfo.AckObject;
            return ackObj.Ack;
        }

        public Task Nack((Envelope Envelope, object AckObject) messageInfo)
        {
            var ackObj = ((Task Ack, Task Nack))messageInfo.AckObject;
            return ackObj.Nack;
        }

        public void Start()
        {
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
                QueueClient queueClient = tokenProvider != null
                    ? new QueueClient(connectionString, queueName, tokenProvider, transportType, receiveMode,
                        retryPolicy)
                    : new QueueClient(connectionString, queueName, receiveMode, retryPolicy);

                queueClient.RegisterSessionHandler(handleMessage, options);
            }
            else
            {
                SubscriptionClient subscriptionClient = tokenProvider != null
                    ? new SubscriptionClient(connectionString, topicName, subscriptionName, tokenProvider,
                        transportType, receiveMode, retryPolicy)
                    : new SubscriptionClient(connectionString, topicName, subscriptionName,
                        receiveMode, retryPolicy);

                subscriptionClient.RegisterSessionHandler(handleMessage, options);
            }
        }

        private Task handleException(ExceptionReceivedEventArgs arg)
        {
            _logger.LogException(arg.Exception,
                message: "Internal failure in the Azure Service Bus QueueClient for " + Address);
            return Task.CompletedTask;
        }

        private async Task handleMessage(IMessageSession session, Message message, CancellationToken cancellationToken)
        {
            string lockToken = message.SystemProperties.LockToken;

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

            await _buffer.SendAsync((envelope, (session.CompleteAsync(lockToken), session.AbandonAsync(lockToken))), cancellationToken);
        }
    }
}
