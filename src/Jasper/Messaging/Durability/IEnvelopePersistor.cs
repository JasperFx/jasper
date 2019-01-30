using System;
using System.IO;
using System.Threading.Tasks;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Durability
{
    public interface IEnvelopeStorageAdmin
    {
        void ClearAllPersistedEnvelopes();
        void RebuildSchemaObjects();
        string CreateSql();
    }


    public interface IEnvelopePersistor
    {
        IEnvelopeStorageAdmin Admin { get; }

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

        void Describe(TextWriter writer);
    }

    public class NulloEnvelopePersistor : IEnvelopePersistor, IEnvelopeStorageAdmin
    {
        public IEnvelopeStorageAdmin Admin => this;

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

    }
}
