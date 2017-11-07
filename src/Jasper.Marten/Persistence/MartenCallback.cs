using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Jasper.Bus.WorkerQueues;
using Marten;

namespace Jasper.Marten
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

        public Task MarkSuccessful()
        {
            // TODO -- later, come back and do retries?
            using (var session = _store.LightweightSession())
            {
                session.Delete(_envelope);
                return session.SaveChangesAsync();
            }
        }

        public Task MarkFailed(Exception ex)
        {
            // Little goofy, not 100% sure that MarkFailed() survives
            return MarkSuccessful();
        }

        public Task MoveToErrors(ErrorReport report)
        {
            // TODO -- let's actually do a dead letter queue inside of Marten
            return MarkSuccessful();
        }

        public async Task Requeue(Envelope envelope)
        {
            using (var session = _store.LightweightSession())
            {
                session.Patch<Envelope>(envelope.Id).Set(x => x.Attempts, envelope.Attempts);
                await session.SaveChangesAsync();
            }

            await _queue.Enqueue(envelope);
        }

        public Task MoveToDelayedUntil(DateTime time, Envelope envelope)
        {
            throw new NotImplementedException();
        }
    }
}
