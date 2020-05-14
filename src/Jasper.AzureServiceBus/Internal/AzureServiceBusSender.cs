using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Logging;
using Jasper.Transports;
using Jasper.Transports.Sending;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Jasper.AzureServiceBus.Internal
{
    public class AzureServiceBusSender : ISender
    {
        private readonly ITransportProtocol<Message> _protocol;
        private readonly AzureServiceBusEndpoint _endpoint;
        private readonly AzureServiceBusTransport _transport;
        private ISenderClient _sender;
        public bool SupportsNativeScheduledSend { get; } = true;
        public Uri Destination => _endpoint.Uri;
        
        public AzureServiceBusSender(AzureServiceBusEndpoint endpoint, AzureServiceBusTransport transport)
        {
            _protocol = endpoint.Protocol;
            _endpoint = endpoint;
            _transport = transport;
            
            // The variance here should be in constructing the sending & buffer blocks
            if (_endpoint.TopicName.IsEmpty())
            {
                _sender = _transport.TokenProvider != null
                    ? new MessageSender(_transport.ConnectionString, _endpoint.QueueName, _transport.TokenProvider,
                        _transport.TransportType, _transport.RetryPolicy)
                    : new MessageSender(_transport.ConnectionString, _endpoint.QueueName, _transport.RetryPolicy);
            }
            else
            {
                _sender = _transport.TokenProvider != null
                    ? new TopicClient(_transport.ConnectionString, _endpoint.TopicName, _transport.TokenProvider,
                        _transport.TransportType, _transport.RetryPolicy)
                    : new TopicClient(_transport.ConnectionString, _endpoint.TopicName,
                        _transport.RetryPolicy);
            }
        }

        public void Dispose()
        {
            _sender?.CloseAsync().GetAwaiter().GetResult();
        }

        public Task Send(Envelope envelope)
        {
            var message = _protocol.WriteFromEnvelope(envelope);
            message.SessionId = Guid.NewGuid().ToString();


            if (envelope.IsDelayed(DateTime.UtcNow))
            {
                return _sender.ScheduleMessageAsync(message, envelope.ExecutionTime.Value);
            }

            return _sender.SendAsync(message);
        }
        
        public async Task<bool> Ping(CancellationToken cancellationToken)
        {
            var envelope = Envelope.ForPing(_endpoint.Uri);
            var message = _protocol.WriteFromEnvelope(envelope);
            message.SessionId = Guid.NewGuid().ToString();

            await _sender.SendAsync(message);

            return true;
        }
    }
}
