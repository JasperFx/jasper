using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Baseline;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports.Lightweight;
using Jasper.Util;

namespace Jasper.LightningDb.Transport
{
    public class PersistentQueues : IReceiverCallback, IDisposable
    {
        private readonly IBusLogger _logger;
        private readonly LightningDbPersistence _persistence;
        private readonly IHandlerPipeline _pipeline;
        private readonly ChannelGraph _channels;
        private readonly CancellationToken _cancellationToken;
        private readonly Dictionary<string, PersistentReceiver> _receivers = new Dictionary<string, PersistentReceiver>();

        public PersistentQueues(IBusLogger logger, LightningDbPersistence persistence, IHandlerPipeline pipeline, ChannelGraph channels, CancellationToken cancellationToken)
        {
            _logger = logger;
            _persistence = persistence;
            _pipeline = pipeline;
            _channels = channels;
            _cancellationToken = cancellationToken;
        }

        public void AddQueue(ChannelNode node)
        {
            var queueName = node.Uri.QueueName();
            if (queueName.IsEmpty()) throw new ArgumentOutOfRangeException(nameof(node), $"Cannot derive the queue name from Uri {node.Uri}");

            _persistence.OpenDatabase(queueName);

            if (!_receivers.ContainsKey(queueName))
            {
                var receiver = new PersistentReceiver(queueName, _persistence, _pipeline, _channels, node);
                _receivers.Add(queueName, receiver);

                receiver.LoadPersisted(_cancellationToken);
            }
        }


        public ReceivedStatus Received(Envelope[] messages)
        {
            if (messages.Any(x => !_receivers.ContainsKey(x.Queue)))
            {
                return ReceivedStatus.QueueDoesNotExist;
            }



            try
            {
                _persistence.StoreInitial(messages);

                foreach (var message in messages)
                {
                    _receivers[message.Queue].Enqueue(message);
                }

                return ReceivedStatus.Successful;
            }
            catch (Exception e)
            {
                _logger.LogException(e);
                return ReceivedStatus.ProcessFailure;
            }
        }

        public void Acknowledged(Envelope[] messages)
        {
            // Nothing
        }

        public void NotAcknowledged(Envelope[] messages)
        {
            _persistence.Remove(messages);
        }

        public void Failed(Exception exception, Envelope[] messages)
        {
            _logger.LogException(new MessageFailureException(messages, exception));
        }

        public void Dispose()
        {
            foreach (var receiver in _receivers.Values)
            {
                receiver.Dispose();
            }

            _receivers.Clear();
        }
    }
}
