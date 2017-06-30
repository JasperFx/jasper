using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports.InMemory;
using Jasper.Util;

namespace Jasper.Bus.Transports.Lightweight
{
    public class LightweightQueues : IReceiverCallback, IDisposable
    {
        private readonly IBusLogger _logger;
        private readonly IInMemoryQueue _queue;
        private readonly IHandlerPipeline _pipeline;
        private readonly ChannelGraph _channels;
        private readonly Dictionary<string, QueueReceiver > _receivers = new Dictionary<string, QueueReceiver >();

        public LightweightQueues(IBusLogger logger, IInMemoryQueue queue, IHandlerPipeline pipeline, ChannelGraph channels)
        {
            _logger = logger;
            _queue = queue;
            _pipeline = pipeline;
            _channels = channels;
        }

        public void Dispose()
        {
            foreach (var receiver in _receivers.Values)
            {
                receiver.Dispose();
            }

            _receivers.Clear();
        }

        public void AddQueue(ChannelNode node)
        {
            var queueName = node.Uri.QueueName();
            if (queueName.IsEmpty()) throw new ArgumentOutOfRangeException(nameof(node), $"Cannot derive the queue name from Uri {node.Uri}");

            if (!_receivers.ContainsKey(queueName))
            {
                var receiver = new QueueReceiver(queueName, _pipeline, _channels, node, _queue);
                _receivers.Add(queueName, receiver);
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
            // Nothing
        }

        public void Failed(Exception exception, Envelope[] messages)
        {
            _logger.LogException(new MessageFailureException(messages, exception));
        }

        public void Enqueue(Envelope envelope)
        {
            _receivers[envelope.Queue].Enqueue(envelope);
        }


    }
}
