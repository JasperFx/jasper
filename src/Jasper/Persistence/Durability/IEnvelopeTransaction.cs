using System.Threading.Tasks;

namespace Jasper.Persistence.Durability
{
    public interface IEnvelopeTransaction
    {
        Task Persist(Envelope envelope);
        Task Persist(Envelope[] envelopes);
        Task ScheduleJob(Envelope envelope);

        Task CopyTo(IEnvelopeTransaction other);
    }
}
