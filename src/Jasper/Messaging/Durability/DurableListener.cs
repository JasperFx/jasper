using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.Transports.Receiving;
using Jasper.Messaging.Transports.Tcp;
using Jasper.Messaging.WorkerQueues;

namespace Jasper.Messaging.Durability
{
    public class DurableListener : IListener
    {
        private readonly IListeningAgent _agent;
        private readonly ITransportLogger _logger;
        private readonly IEnvelopePersistor _persistor;
        private readonly IWorkerQueue _queues;
        private readonly IRetries _retries;
        private readonly MessagingSettings _settings;

        public DurableListener(IListeningAgent agent, IWorkerQueue queues, ITransportLogger logger,
            MessagingSettings settings, IRetries retries, IEnvelopePersistor persistor)
        {
            _agent = agent;
            _queues = queues;
            _logger = logger;
            _settings = settings;
            _retries = retries;
            _persistor = persistor;
        }

        public Uri Address => _agent.Address;

        public async Task<ReceivedStatus> Received(Uri uri, Envelope[] messages)
        {
            var now = DateTime.UtcNow;

            return await ProcessReceivedMessages(now, uri, messages);
        }

        public Task Acknowledged(Envelope[] messages)
        {
            return Task.CompletedTask;
        }

        public Task NotAcknowledged(Envelope[] messages)
        {
            return _persistor.DeleteIncomingEnvelopes(messages);
        }

        public Task Failed(Exception exception, Envelope[] messages)
        {
            _logger.LogException(new MessageFailureException(messages, exception));
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            // nothing
        }

        public void Start()
        {
            _agent.Start(this);
        }

        // Separated for testing here.
        public async Task<ReceivedStatus> ProcessReceivedMessages(DateTime now, Uri uri, Envelope[] messages)
        {
            try
            {
                foreach (var message in messages)
                {
                    message.ReceivedAt = uri;

                    if (message.IsDelayed(now))
                    {
                        message.Status = TransportConstants.Scheduled;
                        message.OwnerId = TransportConstants.AnyNode;
                    }
                    else
                    {
                        message.Status = TransportConstants.Scheduled;
                        message.OwnerId = _settings.UniqueNodeId;
                    }

                    message.Status = message.IsDelayed(now)
                        ? TransportConstants.Scheduled
                        : TransportConstants.Incoming;
                }

                await _persistor.StoreIncoming(messages);


                foreach (var message in messages.Where(x => x.Status == TransportConstants.Incoming))
                {
                    message.Callback = new DurableCallback(message, _queues, _persistor, _retries);
                    await _queues.Enqueue(message);
                }

                _logger.IncomingBatchReceived(messages);

                return ReceivedStatus.Successful;
            }
            catch (Exception e)
            {
                _logger.LogException(e);
                return ReceivedStatus.ProcessFailure;
            }
        }
    }
}
