using System.Threading.Tasks;

namespace Jasper.Persistence.Durability
{
    public interface IDurabilityAgent
    {
        Task EnqueueLocally(Envelope envelope);
        void RescheduleIncomingRecovery();
        void RescheduleOutgoingRecovery();
    }
}
