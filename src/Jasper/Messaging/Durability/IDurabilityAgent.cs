using System.Threading.Tasks;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Durability
{
    public interface IDurabilityAgent
    {
        Task EnqueueLocally(Envelope envelope);
        void RescheduleIncomingRecovery();
        void RescheduleOutgoingRecovery();
    }
}
