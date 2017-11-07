using System;
using System.Threading.Tasks;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Receiving;
using Jasper.Bus.Transports.Tcp;
using Jasper.Bus.Transports.WorkerQueues;
using Marten;

namespace Jasper.Marten
{
    public class MartenBackedListener : IListener
    {
        private readonly IListeningAgent _agent;
        private readonly IWorkerQueue _queues;
        private readonly IDocumentStore _store;
        private readonly CompositeLogger _logger;

        public MartenBackedListener(IListeningAgent agent, IWorkerQueue queues, IDocumentStore store, CompositeLogger logger)
        {
            _agent = agent;
            _queues = queues;
            _store = store;
            _logger = logger;
        }

        public Uri Address => _agent.Address;

        public async Task<ReceivedStatus> Received(Uri uri, Envelope[] messages)
        {
            try
            {
                // TODO -- need to filter on delayed messages here!
                using (var session = _store.LightweightSession())
                {
                    session.Store(messages);
                    await session.SaveChangesAsync();
                }

                // TODO -- HERE, maybe see if you're getting back pressure from too many
                // messages in the queue and shove into the pool of envelopes for anybody to use
                // Instead of sending them into the worker queue

                foreach (var message in messages)
                {
                    message.ReceivedAt = uri;
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
