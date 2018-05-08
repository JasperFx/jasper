using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Durability
{
    public interface IEnvelopeTransaction
    {
        Task Persist(Envelope envelope);
        Task Persist(Envelope[] envelopes);
        Task ScheduleJob(Envelope envelope);

        Task CopyTo(IEnvelopeTransaction other);
    }
}
