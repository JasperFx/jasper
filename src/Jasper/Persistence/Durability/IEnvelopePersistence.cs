using System;
using System.IO;
using System.Threading.Tasks;

namespace Jasper.Persistence.Durability
{
    public interface IEnvelopePersistence : IDisposable
    {
        IEnvelopeStorageAdmin Admin { get; }

        // Used by IRetries and DurableCallback
        Task ScheduleExecution(Envelope[] envelopes);


        // Used by DurableCallback
        Task MoveToDeadLetterStorage(ErrorReport[] errors);

        Task MoveToDeadLetterStorage(Envelope envelope, Exception ex);

        // Used by DurableCallback
        Task IncrementIncomingEnvelopeAttempts(Envelope envelope);

        // Used by LoopbackSendingAgent
        Task StoreIncoming(Envelope envelope);

        // Used by DurableListener and LoopbackSendingAgent
        Task StoreIncoming(Envelope[] envelopes);

        // DurableListener and DurableRetryAgent
        Task DeleteIncomingEnvelopes(Envelope[] envelopes);

        // Used by DurableCallback
        Task DeleteIncomingEnvelope(Envelope envelope);







        void Describe(TextWriter writer);
        Task ScheduleJob(Envelope envelope);

        IDurableStorageSession Session { get; }

        IDurableIncoming Incoming { get; }

        IDurableOutgoing Outgoing { get; }

        Task<Envelope[]> LoadScheduledToExecute(DateTimeOffset utcNow);

        Task ReassignDormantNodeToAnyNode(int nodeId);
        Task<int[]> FindUniqueOwners(int currentNodeId);
    }
}
