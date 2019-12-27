using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Baseline;
using Jasper.Logging;
using Jasper.Transports.Sending;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Jasper.AzureServiceBus.Internal
{
    public class AzureServiceBusSender : ISender
    {
        private readonly IAzureServiceBusProtocol _protocol;
        private readonly AzureServiceBusEndpoint _endpoint;
        private readonly AzureServiceBusTransport _transport;
        private readonly ITransportLogger _logger;
        private readonly CancellationToken _cancellation;
        private ISenderClient _sender;
        private ActionBlock<Envelope> _sending;
        private ISenderCallback _callback;

        public AzureServiceBusSender(AzureServiceBusEndpoint endpoint, AzureServiceBusTransport transport, ITransportLogger logger,
            CancellationToken cancellation)
        {
            _protocol = endpoint.Protocol;
            _endpoint = endpoint;
            _transport = transport;
            _logger = logger;
            _cancellation = cancellation;
            Destination = endpoint.Uri;
        }

        public void Dispose()
        {
            _sender?.CloseAsync().GetAwaiter().GetResult();
        }

        public Uri Destination { get; }
        public int QueuedCount => _sending.InputCount;
        public bool Latched { get; private set; }


        public void Start(ISenderCallback callback)
        {
            _callback = callback;

            // The variance here should be in constructing the sending & buffer blocks
            if (_endpoint.TopicName.IsEmpty())
            {
                _sender = _transport.TokenProvider != null
                    ? new MessageSender(_transport.ConnectionString, _endpoint.QueueName, _transport.TokenProvider,
                        _transport.TransportType, _transport.RetryPolicy)
                    : new MessageSender(_transport.ConnectionString, _endpoint.QueueName, _transport.RetryPolicy);

                _sending = new ActionBlock<Envelope>(sendBySession, new ExecutionDataflowBlockOptions
                {
                    CancellationToken = _cancellation
                });
            }
            else
            {
                _sender = _transport.TokenProvider != null
                    ? new TopicClient(_transport.ConnectionString, _endpoint.TopicName, _transport.TokenProvider,
                        _transport.TransportType, _transport.RetryPolicy)
                    : new TopicClient(_transport.ConnectionString, _endpoint.TopicName,
                        _transport.RetryPolicy);

                _sending = new ActionBlock<Envelope>(sendBySession, new ExecutionDataflowBlockOptions
                {
                    CancellationToken = _cancellation
                });
            }
        }


        public Task Enqueue(Envelope envelope)
        {
            _sending.Post(envelope);

            return Task.CompletedTask;
        }

        public async Task LatchAndDrain()
        {
            Latched = true;

            await _sender.CloseAsync();

            _sending.Complete();

            _logger.CircuitBroken(Destination);
        }

        public void Unlatch()
        {
            _logger.CircuitResumed(Destination);

            Start(_callback);
            Latched = false;
        }

        public Task Ping()
        {
            var envelope = Envelope.ForPing(Destination);
            var message = _protocol.WriteFromEnvelope(envelope);
            message.SessionId = Guid.NewGuid().ToString();

            return _sender.SendAsync(message);
        }

        public bool SupportsNativeScheduledSend { get; } = true;

        private async Task sendBySession(Envelope envelope)
        {
            try
            {
                var message = _protocol.WriteFromEnvelope(envelope);
                message.SessionId = Guid.NewGuid().ToString();


                if (envelope.IsDelayed(DateTime.UtcNow))
                {
                    await _sender.ScheduleMessageAsync(message, envelope.ExecutionTime.Value);
                }
                else
                {
                    await _sender.SendAsync(message);
                }

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
    }
}
