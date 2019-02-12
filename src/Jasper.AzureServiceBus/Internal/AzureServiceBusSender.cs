using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Sending;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Jasper.AzureServiceBus.Internal
{
    public class AzureServiceBusSender : ISender
    {
        private readonly IAzureServiceBusProtocol _protocol;
        private readonly ITransportLogger _logger;
        private readonly CancellationToken _cancellation;
        private readonly IMessageSender _sender;
        private ActionBlock<Envelope> _sending;
        private ActionBlock<Envelope> _serialization;
        private ISenderCallback _callback;

        public AzureServiceBusSender(AzureServiceBusEndpoint endpoint, ITransportLogger logger,
            CancellationToken cancellation)
        {
            _protocol = endpoint.Protocol;
            _logger = logger;
            _cancellation = cancellation;
            Destination = endpoint.Uri.ToUri();


            // TODO -- let's research this a bit. topics will be different
            _sender = endpoint.TokenProvider != null
                ? new MessageSender(endpoint.ConnectionString, endpoint.Uri.QueueName, endpoint.TokenProvider,
                    endpoint.TransportType, endpoint.RetryPolicy)
                : new MessageSender(endpoint.ConnectionString, endpoint.Uri.QueueName, endpoint.RetryPolicy);
        }

        public void Dispose()
        {
            _sender.CloseAsync().GetAwaiter().GetResult();
        }

        public Uri Destination { get; }
        public int QueuedCount => _sending.InputCount;
        public bool Latched { get; private set; }


        public void Start(ISenderCallback callback)
        {
            _callback = callback;

            _serialization = new ActionBlock<Envelope>(e =>
            {
                try
                {
                    e.EnsureData();
                    _sending.Post(e);
                }
                catch (Exception exception)
                {
                    _logger.LogException(exception, e.Id, "Serialization Failure!");
                }
            });

            _sending = new ActionBlock<Envelope>(send, new ExecutionDataflowBlockOptions
            {
                CancellationToken = _cancellation
            });
        }

        public Task Enqueue(Envelope envelope)
        {
            _serialization.Post(envelope);

            return Task.CompletedTask;
        }

        public Task LatchAndDrain()
        {
            Latched = true;

            _sender.CloseAsync().GetAwaiter().GetResult();

            _sending.Complete();
            _serialization.Complete();


            _logger.CircuitBroken(Destination);

            return Task.CompletedTask;
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
            return _sender.SendAsync(message);
        }

        public bool SupportsNativeScheduledSend { get; } = true;

        private async Task send(Envelope envelope)
        {
            try
            {
                var message = _protocol.WriteFromEnvelope(envelope);

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
