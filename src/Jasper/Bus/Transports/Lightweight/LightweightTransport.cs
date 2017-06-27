using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Bus.Configuration;
using Jasper.Bus.Queues;
using Jasper.Bus.Queues.Net;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports.InMemory;
using Jasper.Bus.Transports.LightningQueues;

namespace Jasper.Bus.Transports.Lightweight
{
    public class LightweightTransport : ITransport, ISenderCallback
    {
        private readonly BusSettings _settings;
        private readonly IInMemoryQueue _inmemory;
        private readonly IBusLogger _logger;
        private readonly SendingAgent _sender;
        private readonly Uri _replyUri;
        private readonly Dictionary<int, PortListener> _listeners = new Dictionary<int, PortListener>();
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();

        public LightweightTransport(BusSettings settings, IInMemoryQueue inmemory, IBusLogger logger)
        {
            _settings = settings;
            _inmemory = inmemory;
            _logger = logger;
            _sender = new SendingAgent();
            _replyUri = $"jasper://localhost:{_settings.ResponsePort}/replies".ToUri();

        }

        public void Dispose()
        {
            _cancellation.Cancel();

            _sender.Dispose();

            foreach (var listener in _listeners.Values)
            {
                listener.Dispose();
            }
        }

        public string Protocol { get; } = "jasper";


        public Task Send(Envelope envelope, Uri destination)
        {
            envelope.Destination = destination;
            envelope.ReplyUri = _replyUri;


            var messagePayload = new OutgoingMessage
            {
                Id = MessageId.GenerateRandom(),
                Data = envelope.Data,
                Headers = envelope.Headers,
                SentAt = DateTime.UtcNow,
                Destination = destination,

                // TODO -- this is awful. Let's get this one optimized ASAP
                Queue = envelope.Destination.Segments.Last(),
            };

            //TODO Maybe expose something to modify transport specific payloads?
            messagePayload.TranslateHeaders();

            _sender.Enqueue(messagePayload);

            return Task.CompletedTask;
        }

        public void Start(IHandlerPipeline pipeline, ChannelGraph channels)
        {
            _sender.Start(this);

            channels.AddChannelIfMissing(_replyUri).Incoming = true;

            startListening(pipeline, channels);
        }

        private void startListening(IHandlerPipeline pipeline, ChannelGraph channels)
        {
            var nodes = channels.Where(x => x.Uri.Scheme == Protocol).ToArray();
            if (!nodes.Any()) return;

            var groups = nodes.Where(x => x.Incoming).GroupBy(x => x.Uri.Port);
            foreach (var @group in groups)
            {
                var listener = new PortListener(@group.Key, _inmemory, _logger);
                _listeners.Add(@group.Key, listener);

                foreach (var node in @group)
                {
                    var queueName = node.Uri.Segments.Last();
                    listener.AddQueue(queueName, pipeline, channels, node);
                }
            }

            foreach (var listener in _listeners.Values)
            {
                listener.Start();
            }
        }

        public Uri DefaultReplyUri()
        {
            return _replyUri;
        }

        void ISenderCallback.Successful(OutgoingMessageBatch outgoing)
        {
            // nothing, but might log here, or flip metrics
        }

        void ISenderCallback.TimedOut(OutgoingMessageBatch outgoing)
        {
            processRetry(outgoing);
        }

        void ISenderCallback.SerializationFailure(OutgoingMessageBatch outgoing)
        {
            processRetry(outgoing);
        }

        void ISenderCallback.QueueDoesNotExist(OutgoingMessageBatch outgoing)
        {
            processRetry(outgoing);
        }

        void ISenderCallback.ProcessingFailure(OutgoingMessageBatch outgoing)
        {
            processRetry(outgoing);
        }

        void ISenderCallback.ProcessingFailure(OutgoingMessageBatch outgoing, Exception exception)
        {
            processRetry(outgoing);
        }

        private void processRetry(OutgoingMessageBatch outgoing)
        {
            foreach (var message in outgoing.Messages)
            {
                message.SentAttempts++;
            }

            var groups = outgoing
                .Messages
                .Where(x => x.SentAttempts < _settings.MaximumFireAndForgetSendingAttempts)
                .GroupBy(x => x.SentAttempts);

            foreach (var @group in groups)
            {
                var delayTime = (@group.Key * @group.Key).Seconds();
                var messages = @group.ToArray();
                Task.Delay(delayTime, _cancellation.Token).ContinueWith(_ =>
                {
                    if (_cancellation.IsCancellationRequested)
                    {
                        return;
                    }

                    foreach (var message in messages)
                    {
                        _sender.Enqueue(message);
                    }
                });

            }
        }
    }
}
