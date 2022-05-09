using System.Threading.Tasks;

namespace Jasper.Persistence.Durability;

public interface IEnvelopeTransaction
{
    Task PersistAsync(Envelope envelope);
    Task PersistAsync(Envelope[] envelopes);
    Task ScheduleJobAsync(Envelope envelope);

    Task CopyToAsync(IEnvelopeTransaction other);
}
