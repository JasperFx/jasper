using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Configuration;

namespace Jasper.SqlServer.Persistence
{
    public class SqlServerEnvelopeTransaction : IEnvelopeTransaction, IDisposable
    {
        private readonly SqlTransaction _tx;
        private readonly SqlServerBackedDurableMessagingFactory _persistence;

        public SqlServerEnvelopeTransaction(IMessageContext context, SqlTransaction tx)
        {
            _persistence = context.Advanced.Factory as SqlServerBackedDurableMessagingFactory ?? throw new InvalidOperationException("This message context is not using Sql Server-backed messaging persistence");
            _tx = tx;


        }

        public Task Persist(Envelope envelope)
        {
            return Persist(new Envelope[] {envelope});
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

            return _persistence.StoreIncoming(_tx, new Envelope[] {envelope});
        }

        public Task CopyTo(IEnvelopeTransaction other)
        {


            throw new NotSupportedException($"Cannot copy data from an existing Sql Server envelope transaction to {other}. You may have erroneously enlisted an IMessageContext in a transaction twice.");
        }

        public void Dispose()
        {
            _tx?.Dispose();
        }
    }
}
