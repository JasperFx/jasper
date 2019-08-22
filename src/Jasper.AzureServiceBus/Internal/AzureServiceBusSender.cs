using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Baseline;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Sending;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Jasper.AzureServiceBus.Internal
{
    public class AzureServiceBusSender : ISender
    {
        private readonly object _locker = new object();
        private readonly ConcurrentDictionary<string, ISenderClient> _senders
            = new ConcurrentDictionary<string, ISenderClient>();

        private readonly IAzureServiceBusProtocol _protocol;
        private readonly AzureServiceBusEndpoint _endpoint;
        private readonly ITransportLogger _logger;
        private readonly CancellationToken _cancellation;
        private ISenderClient _sender;
        private ActionBlock<Envelope> _sending;
        private ActionBlock<Envelope> _serialization;
        private ISenderCallback _callback;

        public AzureServiceBusSender(AzureServiceBusEndpoint endpoint, ITransportLogger logger,
            CancellationToken cancellation)
        {
            _protocol = endpoint.Protocol;
            _endpoint = endpoint;
            _logger = logger;
            _cancellation = cancellation;
            Destination = endpoint.Uri.ToUri();
        }

        public void Dispose()
        {
            _sender?.CloseAsync().GetAwaiter().GetResult();

            foreach (var azureServiceBusSender in _senders.Values)
            {
                azureServiceBusSender.CloseAsync().GetAwaiter().GetResult();
            }
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
            }, new ExecutionDataflowBlockOptions{CancellationToken = _cancellation});

            // The variance here should be in constructing the sending & buffer blocks
            if (_endpoint.Uri.TopicName.IsEmpty())
            {
                _sender = _endpoint.TokenProvider != null
                    ? new MessageSender(_endpoint.ConnectionString, _endpoint.Uri.QueueName, _endpoint.TokenProvider,
                        _endpoint.TransportType, _endpoint.RetryPolicy)
                    : new MessageSender(_endpoint.ConnectionString, _endpoint.Uri.QueueName, _endpoint.RetryPolicy);

                _sending = new ActionBlock<Envelope>(sendBySession, new ExecutionDataflowBlockOptions
                {
                    CancellationToken = _cancellation
                });
            }
            else if (_endpoint.Uri.IsMessageSpecificTopic())
            {
                _sending = new ActionBlock<Envelope>(sendByMessageTopicAndSession, new ExecutionDataflowBlockOptions
                {
                    CancellationToken = _cancellation
                });
            }
            else
            {
                _sender = _endpoint.TokenProvider != null
                    ? new TopicClient(_endpoint.ConnectionString, _endpoint.Uri.TopicName, _endpoint.TokenProvider,
                        _endpoint.TransportType, _endpoint.RetryPolicy)
                    : new TopicClient(_endpoint.ConnectionString, _endpoint.Uri.TopicName,
                        _endpoint.RetryPolicy);

                _sending = new ActionBlock<Envelope>(sendBySession, new ExecutionDataflowBlockOptions
                {
                    CancellationToken = _cancellation
                });
            }



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
            message.SessionId = Guid.NewGuid().ToString();

            return _sender.SendAsync(message);
        }

        public bool SupportsNativeScheduledSend { get; } = true;

        private async Task sendBySession(Envelope envelope)
        {
            await sendBySession(envelope, _sender);
        }

        private Task sendByMessageTopicAndSession(Envelope envelope)
        {
            var sender = findTopicSenderFor(envelope);
            return sendBySession(envelope, sender);
        }

        private ISenderClient findTopicSenderFor(Envelope envelope)
        {
            if (_senders.TryGetValue(envelope.MessageType, out var sender))
            {
                return sender;
            }

            lock (_locker)
            {
                if (_senders.TryGetValue(envelope.MessageType, out sender))
                {
                    return sender;
                }

                sender = _endpoint.TokenProvider != null
                    ? new TopicClient(_endpoint.ConnectionString, envelope.MessageType, _endpoint.TokenProvider,
                        _endpoint.TransportType, _endpoint.RetryPolicy)
                    : new TopicClient(_endpoint.ConnectionString, envelope.MessageType,
                        _endpoint.RetryPolicy);

                _senders[envelope.MessageType] = sender;

                return sender;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task sendBySession(Envelope envelope, ISenderClient senderClient)
        {
            try
            {
                var message = _protocol.WriteFromEnvelope(envelope);
                message.SessionId = Guid.NewGuid().ToString();


                if (envelope.IsDelayed(DateTime.UtcNow))
                {
                    await senderClient.ScheduleMessageAsync(message, envelope.ExecutionTime.Value);
                }
                else
                {
                    await senderClient.SendAsync(message);
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
