using System;
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

        public ReceivedStatus Received(Uri uri, Envelope[] messages)
        {
            try
            {
                using (var session = _store.LightweightSession())
                {
                    session.Store(messages);
                    session.SaveChanges();
                }

                foreach (var message in messages)
                {
                    message.ReceivedAt = uri;
                    _queues.Enqueue(message);
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
            // nothing
        }

        public void NotAcknowledged(Envelope[] messages)
        {
            // TODO -- this will get changed to async later
            using (var session = _store.LightweightSession())
            {
                session.Delete(messages);
                session.SaveChanges();
            }
        }

        public void Failed(Exception exception, Envelope[] messages)
        {
            _logger.LogException(new MessageFailureException(messages, exception));
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