using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Jasper.Bus.WorkerQueues;
using Marten;

namespace Jasper.Marten.Persistence
{
    public class MartenCallback : IMessageCallback
    {
        private readonly Envelope _envelope;
        private readonly IWorkerQueue _queue;
        private readonly IDocumentStore _store;

        public MartenCallback(Envelope envelope, IWorkerQueue queue, IDocumentStore store)
        {
            _envelope = envelope;
            _queue = queue;
            _store = store;
        }

        public async Task MarkComplete()
        {
            // TODO -- later, come back and do retries?
            using (var session = _store.LightweightSession())
            {
                session.Delete(_envelope);
                await session.SaveChangesAsync();
            }
        }

        public async Task MoveToErrors(Envelope envelope, Exception exception)
        {
            // TODO -- later, come back and do retries?
            using (var session = _store.LightweightSession())
            {
                session.Delete(_envelope);

                var report = new ErrorReport(envelope, exception);
                session.Store(report);

                await session.SaveChangesAsync();
            }

            // TODO -- make this configurable about whether or not it saves off error reports

        }

        public async Task Requeue(Envelope envelope)
        {
            // TODO -- Optimize by incrementing instead w/ sql
            using (var session = _store.LightweightSession())
            {
                envelope.Attempts++;
                session.Store(envelope);

                await session.SaveChangesAsync();
            }

            await _queue.Enqueue(envelope);
        }

        public async Task MoveToDelayedUntil(DateTime time, Envelope envelope)
        {
            envelope.ExecutionTime = time;
            envelope.Status = TransportConstants.Scheduled;

            using (var session = _store.LightweightSession())
            {
                session.Store(envelope);
                await session.SaveChangesAsync();
            }
        }
    }
}
