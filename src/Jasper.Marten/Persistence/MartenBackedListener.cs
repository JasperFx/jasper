using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Marten.Persistence.Operations;
using Jasper.Marten.Persistence.Resiliency;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.Transports.Receiving;
using Jasper.Messaging.Transports.Tcp;
using Jasper.Messaging.WorkerQueues;
using Marten;

namespace Jasper.Marten.Persistence
{
    public class MartenBackedListener : IListener
    {
        private readonly IListeningAgent _agent;
        private readonly IWorkerQueue _queues;
        private readonly IDocumentStore _store;
        private readonly ITransportLogger _logger;
        private readonly MessagingSettings _settings;
        private readonly EnvelopeTables _marker;
        private readonly IRetries _retries;
        private MartenEnvelopePersistor _persistor;

        public MartenBackedListener(IListeningAgent agent, IWorkerQueue queues, IDocumentStore store, ITransportLogger logger, MessagingSettings settings, EnvelopeTables marker, IRetries retries)
        {
            _agent = agent;
            _queues = queues;
            _store = store;
            _logger = logger;
            _settings = settings;
            _marker = marker;
            _retries = retries;

            _persistor = new MartenEnvelopePersistor(_store, _marker);
        }

        public Uri Address => _agent.Address;

        public async Task<ReceivedStatus> Received(Uri uri, Envelope[] messages)
        {
            var now = DateTime.UtcNow;

            return await ProcessReceivedMessages(now, uri, messages);
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

                using (var session = _store.LightweightSession())
                {
                    session.StoreIncoming(_marker, messages);
                    await session.SaveChangesAsync();
                }

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

        public Task Acknowledged(Envelope[] messages)
        {
            return Task.CompletedTask;
        }

        public async Task NotAcknowledged(Envelope[] messages)
        {
            using (var session = _store.LightweightSession())
            {
                session.Delete(messages);
                await session.SaveChangesAsync();
            }
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
    }
}
