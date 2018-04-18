using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Durability
{
    public interface IEnvelopePersistor
    {
        Task DeleteIncomingEnvelopes(Envelope[] envelopes);
        Task DeleteOutgoingEnvelopes(Envelope[] envelopes);
        Task DeleteOutgoingEnvelope(Envelope envelope);
        Task MoveToDeadLetterStorage(ErrorReport[] errors);
        Task ScheduleExecution(Envelope[] envelopes);

        Task<ErrorReport> LoadDeadLetterEnvelope(Guid id);
        Task IncrementIncomingEnvelopeAttempts(Envelope envelope);

        Task StoreIncoming(Envelope envelope);
        Task StoreIncoming(IEnumerable<Envelope> envelopes);

        Task DiscardAndReassignOutgoing(Envelope[] discards, Envelope[] reassigned, int nodeId);

        Task StoreOutgoing(Envelope envelope, int ownerId);

        Task StoreOutgoing(Envelope[] envelopes, int ownerId);
    }
}
