using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Queues;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports.InMemory;

namespace Jasper.Bus.Transports.Lightweight
{
    public class PortListener : IReceiverCallback, IDisposable
    {
        private readonly IInMemoryQueue _inmemory;
        private readonly IBusLogger _logger;
        private readonly Dictionary<string, QueueReceiver> _receivers = new Dictionary<string, QueueReceiver>();
        private readonly ListeningAgent _listener;

        public PortListener(int port, IInMemoryQueue inmemory, IBusLogger logger)
        {
            _inmemory = inmemory;
            _logger = logger;
            _listener = new ListeningAgent(this, port);
        }

        public void Start()
        {
            _listener.Start();
        }

        public void AddQueue(string queueName, IHandlerPipeline pipeline, ChannelGraph channels, ChannelNode node)
        {
            _receivers.Add(queueName, new QueueReceiver(queueName, pipeline, channels, node, _inmemory));
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

        public void Dispose()
        {
            foreach (var receiver in _receivers.Values)
            {
                receiver.Dispose();
            }

            _listener?.Dispose();
        }
    }

    public class MessageFailureException : Exception
    {
        public MessageFailureException(Envelope[] messages, Exception innerException) : base($"SEE THE INNER EXCEPTION -- Failed on messages {messages.Select(x => x.ToString()).Join(", ")}", innerException)
        {
        }
    }
}
