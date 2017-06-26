using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Bus.Configuration;
using Jasper.Bus.Queues;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports.InMemory;

namespace Jasper.Bus.Transports.Lightweight
{
    public class PortListener : IReceiverCallback, IDisposable
    {
        private readonly IInMemoryQueue _inmemory;
        private readonly Dictionary<string, QueueReceiver> _receivers = new Dictionary<string, QueueReceiver>();
        private readonly ListeningAgent _listener;

        public PortListener(int port, IInMemoryQueue inmemory)
        {
            _inmemory = inmemory;
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

        public ReceivedStatus Received(Message[] messages)
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
                // TODO -- need to log here
                return ReceivedStatus.ProcessFailure;
            }
        }

        public void Acknowledged(Message[] messages)
        {
            // Nothing
        }

        public void NotAcknowledged(Message[] messages)
        {
            // Nothing
        }

        public void Failed(Exception exception, Message[] messages)
        {
            // TODO -- log the exception at least
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
}
