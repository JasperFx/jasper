using System.Threading.Tasks;

namespace Jasper.Persistence.Durability
{
    public interface IDurabilityAgent
    {
        Task EnqueueLocallyAsync(Envelope envelope);
        void RescheduleIncomingRecovery();
        void RescheduleOutgoingRecovery();
    }
}
