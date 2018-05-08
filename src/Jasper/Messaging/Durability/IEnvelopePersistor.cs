using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Durability
{
    public interface IEnvelopePersistor
    {
        Task DeleteIncomingEnvelopes(Envelope[] envelopes);
        Task DeleteIncomingEnvelope(Envelope envelope);
        Task DeleteOutgoingEnvelopes(Envelope[] envelopes);
        Task DeleteOutgoingEnvelope(Envelope envelope);
        Task MoveToDeadLetterStorage(ErrorReport[] errors);
        Task ScheduleExecution(Envelope[] envelopes);

        Task<ErrorReport> LoadDeadLetterEnvelope(Guid id);
        Task IncrementIncomingEnvelopeAttempts(Envelope envelope);

        Task StoreIncoming(Envelope envelope);
        Task StoreIncoming(Envelope[] envelopes);

        Task DiscardAndReassignOutgoing(Envelope[] discards, Envelope[] reassigned, int nodeId);

        Task StoreOutgoing(Envelope envelope, int ownerId);

        Task StoreOutgoing(Envelope[] envelopes, int ownerId);

        Task<PersistedCounts> GetPersistedCounts();
    }

    public class NulloEnvelopePersistor : IEnvelopePersistor
    {
        public Task DeleteIncomingEnvelopes(Envelope[] envelopes)
        {
            throw new NotImplementedException();
        }

        public Task DeleteIncomingEnvelope(Envelope envelope)
        {
            throw new NotImplementedException();
        }

        public Task DeleteOutgoingEnvelopes(Envelope[] envelopes)
        {
            throw new NotImplementedException();
        }

        public Task DeleteOutgoingEnvelope(Envelope envelope)
        {
            throw new NotImplementedException();
        }

        public Task MoveToDeadLetterStorage(ErrorReport[] errors)
        {
            throw new NotImplementedException();
        }

        public Task ScheduleExecution(Envelope[] envelopes)
        {
            throw new NotImplementedException();
        }

        public Task<ErrorReport> LoadDeadLetterEnvelope(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task IncrementIncomingEnvelopeAttempts(Envelope envelope)
        {
            throw new NotImplementedException();
        }

        public Task StoreIncoming(Envelope envelope)
        {
            throw new NotImplementedException();
        }

        public Task StoreIncoming(Envelope[] envelopes)
        {
            throw new NotImplementedException();
        }

        public Task DiscardAndReassignOutgoing(Envelope[] discards, Envelope[] reassigned, int nodeId)
        {
            throw new NotImplementedException();
        }

        public Task StoreOutgoing(Envelope envelope, int ownerId)
        {
            throw new NotImplementedException();
        }

        public Task StoreOutgoing(Envelope[] envelopes, int ownerId)
        {
            throw new NotImplementedException();
        }

        public Task<PersistedCounts> GetPersistedCounts()
        {
            // Nothing to do, but keeps the metrics from blowing up
            return Task.FromResult(new PersistedCounts());
        }
    }
}
