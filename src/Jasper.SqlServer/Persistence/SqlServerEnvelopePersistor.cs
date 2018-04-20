using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;

namespace Jasper.SqlServer.Persistence
{
    public class SqlServerEnvelopePersistor : IEnvelopePersistor
    {
        private readonly SqlServerSettings _settings;

        public SqlServerEnvelopePersistor(SqlServerSettings settings)
        {
            _settings = settings;
        }

        public Task DeleteIncomingEnvelopes(Envelope[] envelopes)
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

        public Task StoreIncoming(IEnumerable<Envelope> envelopes)
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
    }
}
