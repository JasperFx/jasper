using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Queues;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports.Lightweight;

namespace Jasper.LightningDb.Transport
{
    public class PersistentPortListener : IReceiverCallback, IDisposable
    {
        private readonly IBusLogger _logger;
        private readonly ListeningAgent _listener;
        private readonly Dictionary<string, PersistentQueueReceiver> _receivers = new Dictionary<string, PersistentQueueReceiver>();

        public PersistentPortListener(int port, IBusLogger logger)
        {
            _logger = logger;
            _listener = new ListeningAgent(this, port);
        }

        public void Start()
        {
            _listener.Start();
        }

        public void AddQueue(string queueName, IHandlerPipeline pipeline, ChannelGraph channels, ChannelNode node)
        {
            _receivers.Add(queueName, new PersistentQueueReceiver(queueName, pipeline, channels, node));
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
                // TODO -- need to delete the messages from the DB
                _logger.LogException(e);
                return ReceivedStatus.ProcessFailure;
            }
        }

        public void Acknowledged(Envelope[] messages)
        {
            // TODO -- NOTHING according to LQ
            throw new NotImplementedException();
        }

        public void NotAcknowledged(Envelope[] messages)
        {
            // TODO -- delete the messages 'cause you never really got them
            throw new NotImplementedException();
        }

        public void Failed(Exception exception, Envelope[] messages)
        {
            // TODO -- delete the messages 'cause you never really got them
            throw new NotImplementedException();
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
