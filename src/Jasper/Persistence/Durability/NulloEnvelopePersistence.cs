using System;
using System.IO;
using System.Threading.Tasks;
using Jasper.Logging;

namespace Jasper.Persistence.Durability
{
    public class NulloEnvelopePersistence : IEnvelopePersistence, IEnvelopeStorageAdmin
    {
        public IEnvelopeStorageAdmin Admin => this;
        public IDurabilityAgentStorage AgentStorage { get; } = null;

        public Task DeleteIncomingEnvelopes(Envelope[] envelopes)
        {
            return Task.CompletedTask;
        }

        public Task DeleteIncomingEnvelope(Envelope envelope)
        {
            return Task.CompletedTask;
        }

        public Task DeleteOutgoing(Envelope[] envelopes)
        {
            return Task.CompletedTask;
        }

        public Task DeleteOutgoing(Envelope envelope)
        {
            return Task.CompletedTask;
        }

        public Task MoveToDeadLetterStorage(ErrorReport[] errors)
        {
            return Task.CompletedTask;
        }

        public Task ScheduleExecution(Envelope[] envelopes)
        {
            return Task.CompletedTask;
        }

        public Task<ErrorReport> LoadDeadLetterEnvelope(Guid id)
        {
            return Task.FromResult<ErrorReport>(null);
        }

        public Task<Envelope[]> AllIncomingEnvelopes()
        {
            return Task.FromResult(new Envelope[0]);
        }

        public Task<Envelope[]> AllOutgoingEnvelopes()
        {
            return Task.FromResult(new Envelope[0]);
        }

        public void ReleaseAllOwnership()
        {
        }

        public Task IncrementIncomingEnvelopeAttempts(Envelope envelope)
        {
            return Task.CompletedTask;
        }

        public Task StoreIncoming(Envelope envelope)
        {
            return Task.CompletedTask;
        }

        public Task StoreIncoming(Envelope[] envelopes)
        {
            return Task.CompletedTask;
        }

        public Task DiscardAndReassignOutgoing(Envelope[] discards, Envelope[] reassigned, int nodeId)
        {
            return Task.CompletedTask;
        }

        public Task StoreOutgoing(Envelope envelope, int ownerId)
        {
            return Task.CompletedTask;
        }

        public Task StoreOutgoing(Envelope[] envelopes, int ownerId)
        {
            return Task.CompletedTask;
        }

        public Task<PersistedCounts> GetPersistedCounts()
        {
            // Nothing to do, but keeps the metrics from blowing up
            return Task.FromResult(new PersistedCounts());
        }

        public void Describe(TextWriter writer)
        {
            writer.WriteLine("No persistent envelope storage");
        }

        public void ClearAllPersistedEnvelopes()
        {
            Console.WriteLine("There is no durable envelope storage");
        }

        public void RebuildSchemaObjects()
        {
            Console.WriteLine("There is no durable envelope storage");
        }

        public string CreateSql()
        {
            Console.WriteLine("There is no durable envelope storage");
            return string.Empty;
        }

        public Task ScheduleJob(Envelope envelope)
        {
            throw new NotSupportedException();
        }


    }
}
