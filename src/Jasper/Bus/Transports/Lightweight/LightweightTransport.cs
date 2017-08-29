using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Bus.Configuration;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports.InMemory;

namespace Jasper.Bus.Transports.Lightweight
{
    public class LightweightTransport : ITransport, ISenderCallback
    {
        private readonly BusSettings _settings;
        private readonly ILoopbackQueue _inmemory;
        private readonly IBusLogger _logger;
        private readonly SendingAgent _sender;
        private readonly Uri _replyUri;
        private readonly IList<ListeningAgent> _listeners = new List<ListeningAgent>();
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
        private LightweightQueues _queues;

        public LightweightTransport(BusSettings settings, ILoopbackQueue inmemory, CompositeLogger logger)
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
            _queues.Dispose();
            _sender.Dispose();

            foreach (var listener in _listeners)
            {
                listener.Dispose();
            }

            _listeners.Clear();
        }

        public string Protocol { get; } = "jasper";


        public Task Send(Envelope envelope, Uri destination)
        {
            envelope.Destination = destination;
            envelope.ReplyUri = _replyUri;

            _sender.Enqueue(envelope);

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
            var nodes = channels.Where(x => x.Uri.Scheme == Protocol).Distinct().ToArray();
            if (!nodes.Any()) return;


            _queues = new LightweightQueues(_logger, _inmemory, pipeline, channels);
            foreach (var node in nodes)
            {
                _queues.AddQueue(node);
            }


            foreach (var port in nodes.Where(x => x.Incoming).Select(x => x.Uri.Port).Distinct())
            {
                var agent = new ListeningAgent(_queues, port);
                _listeners.Add(agent);

                agent.Start();
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

            var expired = outgoing.Messages.Where(x => x.SentAttempts >= _settings.MaximumFireAndForgetSendingAttempts);
            foreach (var envelope in expired)
            {
                _logger.Undeliverable(envelope);
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
