using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Util;

namespace Jasper.Bus.Transports.Core
{
    public abstract class TransportBase: ITransport, ISenderCallback
    {
        public string Protocol { get; }
        public CompositeLogger Logger { get; }
        public IPersistence Persistence { get; }

        private TransportSettings _settings;

        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
        private readonly SendingAgent _sender;
        private Uri _replyUri;
        private ListeningAgent _listener;
        private QueueCollection _queues;

        protected TransportBase(string protocol, IPersistence persistence, CompositeLogger logger, ISenderProtocol sendingProtocol)
        {
            _sender = new SendingAgent(sendingProtocol);
            Protocol = protocol;
            Persistence = persistence;
            Logger = logger;
        }

        public void Dispose()
        {
            _cancellation.Cancel();

            _sender?.Dispose();

            _listener?.Dispose();

            _queues?.Dispose();
        }

        private void processRetry(OutgoingMessageBatch outgoing)
        {
            foreach (var message in outgoing.Messages)
            {
                message.SentAttempts++;
            }

            var groups = outgoing
                .Messages
                .Where(x => x.SentAttempts < _settings.MaximumSendAttempts)
                .GroupBy(x => x.SentAttempts);

            foreach (var group in groups)
            {
                var delayTime = (group.Key * group.Key).Seconds();
                var messages = group.ToArray();

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

            Persistence.PersistBasedOnSentAttempts(outgoing, _settings.MaximumSendAttempts);
        }

        void ISenderCallback.Successful(OutgoingMessageBatch outgoing)
        {
            Persistence.RemoveOutgoing(outgoing.Messages);
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


        public Task Send(Envelope envelope, Uri destination)
        {
            envelope.Destination = destination;
            enqueue(envelope);

            return Task.CompletedTask;
        }

        private void enqueue(Envelope envelope)
        {
            envelope.ReplyUri = _replyUri;

            Persistence.StoreOutgoing(envelope);

            _sender.Enqueue(envelope);
        }


        public Uri DefaultReplyUri()
        {
            return _replyUri;
        }

        public TransportState State => _settings.State;

        public IChannel[] Start(IHandlerPipeline pipeline, BusSettings settings, OutgoingChannels channels)
        {
            _settings = settings[Protocol];

            if (_settings.State == TransportState.Disabled) return new IChannel[0];




            var provider = buildQueueProvider(channels);

            _sender.Start(this);

            _queues = new QueueCollection(Logger, provider, pipeline, _cancellation.Token);

            var queueNames = _settings.AllQueueNames();

            Persistence.Initialize(queueNames);

            foreach (var queue in _settings)
            {
                _queues.AddQueue(queue.Name, queue.Parallelization);
            }

            if (_settings.Port.HasValue)
            {
                _replyUri = $"{Protocol}://{settings.MachineName}:{_settings.Port}/{TransportConstants.Replies}"
                    .ToUri();


                _listener = new ListeningAgent(_queues, _settings.Port.Value, Protocol, _cancellation.Token);
                _listener.Start();
            }

            Persistence.RecoverOutgoingMessages(enqueue, _cancellation.Token);
            Persistence.RecoverPersistedMessages(queueNames, env => _queues.Enqueue(env.Queue, env), _cancellation.Token);

            return settings.KnownSubscribers.Where(x => x.Uri.Scheme == Protocol)
                .Select(x => new Channel(x, this)).OfType<IChannel>().ToArray();
        }

        protected abstract IQueueProvider buildQueueProvider(OutgoingChannels channels);

    }
}
