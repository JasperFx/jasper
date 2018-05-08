using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Durability
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

        public Task Persist(Envelope[] envelopes)
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
            await other.Persist(Queued.ToArray());

            foreach (var envelope in Scheduled)
            {
                await other.ScheduleJob(envelope);
            }
        }
    }
}
