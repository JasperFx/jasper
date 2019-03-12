using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;

namespace Jasper.Persistence.SqlServer.Persistence
{
    public class SqlServerEnvelopeTransaction : IEnvelopeTransaction, IDisposable
    {
        private readonly SqlServerEnvelopePersistence _persistence;
        private readonly SqlTransaction _tx;

        public SqlServerEnvelopeTransaction(IMessageContext context, SqlTransaction tx)
        {
            _persistence = context.Advanced.Persistence as SqlServerEnvelopePersistence ??
                           throw new InvalidOperationException(
                               "This message context is not using Sql Server-backed messaging persistence");
            _tx = tx;
        }

        public void Dispose()
        {
            _tx?.Dispose();
        }

        public Task Persist(Envelope envelope)
        {
            return Persist(new[] {envelope});
        }

        public Task Persist(Envelope[] envelopes)
        {
            if (!envelopes.Any()) return Task.CompletedTask;

            return _persistence.StoreOutgoing(_tx, envelopes);
        }

        public Task ScheduleJob(Envelope envelope)
        {
            envelope.OwnerId = TransportConstants.AnyNode;
            envelope.Status = TransportConstants.Scheduled;

            return _persistence.StoreIncoming(_tx, new[] {envelope});
        }

        public Task CopyTo(IEnvelopeTransaction other)
        {
            throw new NotSupportedException(
                $"Cannot copy data from an existing Sql Server envelope transaction to {other}. You may have erroneously enlisted an IMessageContext in a transaction twice.");
        }
    }
}
