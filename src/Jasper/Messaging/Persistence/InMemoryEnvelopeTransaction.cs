using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Persistence
{
    public class InMemoryEnvelopeTransaction : IEnvelopeTransaction
    {
        public readonly IList<Envelope> Queued = new List<Envelope>();
        public readonly IList<Envelope> Scheduled = new List<Envelope>();

        public Task Persist(Envelope envelope)
        {
            Queued.Fill(envelope);
            return Task.CompletedTask;
        }

        public Task Persist(IEnumerable<Envelope> envelopes)
        {
            Queued.Fill(envelopes);
            return Task.CompletedTask;
        }

        public Task ScheduleJob(Envelope envelope)
        {
            Scheduled.Fill(envelope);
            return Task.CompletedTask;
        }

        public async Task CopyTo(IEnvelopeTransaction other)
        {
            await other.Persist(Queued);

            foreach (var envelope in Scheduled)
            {
                await other.ScheduleJob(envelope);
            }
        }
    }
}
