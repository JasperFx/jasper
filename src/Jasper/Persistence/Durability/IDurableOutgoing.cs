using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace Jasper.Persistence.Durability
{
    public interface IDurableOutgoing
    {
        Task<Envelope[]> Load(Uri destination);
        Task Reassign(int ownerId, Envelope[] outgoing);
        Task DeleteByDestination(Uri destination);
        Task Delete(Envelope[] outgoing);
        Task<Uri[]> FindAllDestinations();









        // Used by DurableRetryAgent, could go to IDurabilityAgent
        Task DiscardAndReassignOutgoing(Envelope[] discards, Envelope[] reassigned, int nodeId);

        // Used by DurableSendingAgent, could go to durability agent
        Task StoreOutgoing(Envelope envelope, int ownerId);

        // Used by DurableSendingAgent
        Task StoreOutgoing(Envelope[] envelopes, int ownerId);

        // Used by DurableSendingAgent
        Task DeleteOutgoing(Envelope[] envelopes);

        // Used by DurableSendingAgent
        Task DeleteOutgoing(Envelope envelope);


        Task StoreOutgoing(DbTransaction tx, Envelope[] envelopes);
    }
}
