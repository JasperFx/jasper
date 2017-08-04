using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Lightweight;

namespace Jasper.LightningDb.Transport
{
    public class PersistentTransport : ITransport, ISenderCallback
    {
        public static string MaxAttemptsHeader = "max-delivery-attempts";
        public static string DeliverByHeader = "deliver-by";

        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
        private readonly IList<ListeningAgent> _listeners = new List<ListeningAgent>();
        private readonly IBusLogger _logger;
        private readonly LightningDbSettings _lqSettings;
        private LightningDbPersistence _persistence;
        private readonly SendingAgent _sender = new SendingAgent();
        private readonly BusSettings _settings;
        private PersistentQueues _queues;
        private Uri _replyUri;
        private Task _persistedOutgoing;

        public PersistentTransport(BusSettings settings, LightningDbSettings lqSettings, CompositeLogger logger)
        {
            _settings = settings;
            _lqSettings = lqSettings;
            _logger = logger;

        }

        void ISenderCallback.Successful(OutgoingMessageBatch outgoing)
        {
            _persistence.Remove(LightningDbPersistence.Outgoing, outgoing.Messages);
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

        public void Dispose()
        {
            _cancellation.Cancel();

            _sender?.Dispose();

            foreach (var listener in _listeners)
            {
                listener.Dispose();
            }

            _queues?.Dispose();

            _persistence?.Dispose();
        }

        public string Protocol => "lq.tcp";

        public Task Send(Envelope envelope, Uri destination)
        {
            envelope.Destination = destination;
            envelope.ReplyUri = _replyUri;

            _persistence.Store(LightningDbPersistence.Outgoing, envelope);

            _sender.Enqueue(envelope);

            return Task.CompletedTask;
        }

        public void Start(IHandlerPipeline pipeline, ChannelGraph channels)
        {
            var nodes = channels.Where(x => x.Uri.Scheme == Protocol).ToArray();
            if (!nodes.Any())
            {
                return;
            }


            _sender.Start(this);

            _persistence = new LightningDbPersistence(_lqSettings);

            chooseReplyNode(channels, nodes);

            startListening(pipeline, channels);
        }

        public Uri DefaultReplyUri()
        {
            return _replyUri;
        }

        public static void DeleteAllStorage(string queuePath = null)
        {
            var fileSystem = new FileSystem();
            if (queuePath == null)
            {
                //Find all queues matching queuePath regardless of port.
                var jasperQueuePath = new LightningDbSettings().QueuePath;
                queuePath = fileSystem.GetDirectory(jasperQueuePath);

                var queues = fileSystem
                    .ChildDirectoriesFor(queuePath)
                    .Where(x => x.StartsWith(jasperQueuePath, StringComparison.OrdinalIgnoreCase));

                queues.Each(x => fileSystem.DeleteDirectory(x));
            }
            else
            {
                fileSystem.DeleteDirectory(queuePath);
            }
        }

        private void startListening(IHandlerPipeline pipeline, ChannelGraph channels)
        {
            var nodes = channels.Where(x => x.Uri.Scheme == Protocol).ToArray();
            if (!nodes.Any()) return;


            _queues = new PersistentQueues(_logger, _persistence, pipeline, channels, _cancellation.Token);
            foreach (var node in nodes)
            {
                _queues.AddQueue(node);
            }


            foreach (var port in nodes.Select(x => x.Uri.Port).Distinct())
            {
                var agent = new ListeningAgent(_queues, port);
                _listeners.Add(agent);

                agent.Start();
            }

            _persistedOutgoing = Task.Factory.StartNew(() =>
            {
                _persistence.ReadAll(LightningDbPersistence.Outgoing, env => Send(env, env.Destination));
            }, _cancellation.Token);
        }

        private void chooseReplyNode(ChannelGraph channels, ChannelNode[] nodes)
        {
            var replyNode = nodes.FirstOrDefault(x => x.Incoming);
            if (replyNode == null)
            {
                _replyUri = $"lq.tcp://localhost{_lqSettings.DefaultReplyPort}/replies".ToUri();
                channels.AddChannelIfMissing(_replyUri).Incoming = true;
            }
            else
            {
                _replyUri = replyNode.Destination;
            }
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

            _persistence.PersistBasedOnSentAttempts(outgoing, _lqSettings.MaximumSendAttempts);
        }
    }
}
