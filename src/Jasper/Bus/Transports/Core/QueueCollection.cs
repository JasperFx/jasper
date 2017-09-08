using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports.Configuration;
using Jasper.Util;

namespace Jasper.Bus.Transports.Core
{
    public class QueueCollection : IReceiverCallback, IDisposable
    {
        private readonly IBusLogger _logger;
        private readonly IHandlerPipeline _pipeline;
        private readonly CancellationToken _cancellationToken;
        private readonly Dictionary<string, QueueReceiver> _receivers
            = new Dictionary<string, QueueReceiver>();

        private readonly IQueueProvider _provider;
        private readonly QueueReceiver _default;

        public QueueCollection(IBusLogger logger, IQueueProvider provider, IHandlerPipeline pipeline, CancellationToken cancellationToken)
        {
            _logger = logger;
            _pipeline = pipeline;
            _cancellationToken = cancellationToken;
            _provider = provider;
        }

        public QueueReceiver AddQueue(string queueName, int parallelization)
        {
            if (!_receivers.ContainsKey(queueName))
            {
                var receiver = new QueueReceiver(_pipeline, queueName, parallelization, _provider, _cancellationToken);
                _receivers.Add(queueName, receiver);

                return receiver;
            }

            return _receivers[queueName];
        }

        public bool Has(string queueName)
        {
            return _receivers.ContainsKey(queueName);
        }

        public void Enqueue(Uri destination, Envelope envelope)
        {
            Enqueue(destination.QueueName(), envelope);
        }

        public void Enqueue(string queueName, Envelope envelope)
        {
            if (_receivers.ContainsKey(queueName))
            {
                _receivers[queueName].Enqueue(envelope);
            }
            else
            {
                _default.Enqueue(envelope);
            }
        }


        public ReceivedStatus Received(Uri uri, Envelope[] messages)
        {
            // NOTE! We no longer validate against queues not existing.
            // instead, we just shuttle them to the default queue
            try
            {
                _provider.StoreIncomingMessages(messages);

                foreach (var message in messages)
                {
                    message.ReceivedAt = uri;
                    Enqueue(message.Queue, message);
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
            _provider.RemoveIncomingMessages(messages);
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

        public void AddQueues(TransportSettings settings)
        {
            foreach (var queue in settings)
            {
                AddQueue(queue.Name, queue.Parallelization);
            }
        }
    }
}
