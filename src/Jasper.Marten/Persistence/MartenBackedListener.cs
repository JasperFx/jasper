using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Receiving;
using Jasper.Bus.Transports.Tcp;
using Jasper.Bus.WorkerQueues;
using Marten;

namespace Jasper.Marten.Persistence
{
    public class MartenBackedListener : IListener
    {
        private readonly IListeningAgent _agent;
        private readonly IWorkerQueue _queues;
        private readonly IDocumentStore _store;
        private readonly CompositeLogger _logger;
        private readonly BusSettings _settings;

        public MartenBackedListener(IListeningAgent agent, IWorkerQueue queues, IDocumentStore store, CompositeLogger logger, BusSettings settings)
        {
            _agent = agent;
            _queues = queues;
            _store = store;
            _logger = logger;
            _settings = settings;
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
                    session.Store(messages);
                    await session.SaveChangesAsync();
                }

                // TODO -- HERE, maybe see if you're getting back pressure from too many
                // messages in the queue and shove into the pool of envelopes for anybody to use
                // Instead of sending them into the worker queue

                foreach (var message in messages.Where(x => x.Status == TransportConstants.Incoming))
                {
                    message.Callback = new MartenCallback(message, _queues, _store);
                    await _queues.Enqueue(message);
                }

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
