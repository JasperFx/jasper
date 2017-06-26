using System;
using System.Collections.Generic;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus.Queues.New.Lightweight
{
    public class PortListener : IReceiverCallback, IDisposable
    {
        private readonly Dictionary<string, QueueReceiver> _receivers = new Dictionary<string, QueueReceiver>();
        private readonly ListeningAgent _listener;

        public PortListener(int port)
        {
            _listener = new ListeningAgent(this, port);
        }

        public void Start()
        {
            _listener.Start();
        }

        public void AddQueue(string queueName, IHandlerPipeline pipeline, ChannelGraph channels, ChannelNode node)
        {
            _receivers.Add(queueName, new QueueReceiver(queueName, pipeline, channels, node));
        }

        public ReceivedStatus Received(Message[] messages)
        {
            // TODO -- needs to delegate to the queue receivers
            throw new NotImplementedException();
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
