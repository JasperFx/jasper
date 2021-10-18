using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Persistence.Database;
using Jasper.Persistence.Postgresql.Util;
using Jasper.Transports;
using Jasper.Util;
using NpgsqlTypes;
using Weasel.Core;

namespace Jasper.Persistence.Postgresql
{
    public class PostgresqlDurableOutgoing : DurableOutgoing
    {
        private readonly DurableStorageSession _session;
        private readonly string _deleteOutgoingSql;
        private readonly string _reassignSql;
        private readonly string _deleteOutgoingEnvelopesSql;
        private readonly CancellationToken _cancellation;

        public PostgresqlDurableOutgoing(DurableStorageSession session, DatabaseSettings settings, AdvancedSettings options)
            : base(session, settings, options)
        {
            _session = session;

            _deleteOutgoingSql =
                $"delete from {settings.SchemaName}.{DatabaseConstants.OutgoingTable} where id = ANY(@ids)";

            _reassignSql = $"update {settings.SchemaName}.{DatabaseConstants.OutgoingTable} set owner_id = @owner where id = ANY(@ids)";
            _deleteOutgoingEnvelopesSql = $"delete from {settings.SchemaName}.{DatabaseConstants.OutgoingTable} WHERE id = ANY(@ids);";
            _cancellation = options.Cancellation;
        }

        public override Task DiscardAndReassignOutgoing(Envelope[] discards, Envelope[] reassigned, int nodeId)
        {
            return DatabaseSettings.CreateCommand(_deleteOutgoingEnvelopesSql +
                                                  $";update {DatabaseSettings.SchemaName}.{DatabaseConstants.OutgoingTable} set owner_id = @node where id = ANY(@rids)")
                .With("ids", discards)
                .With("node", nodeId)
                .With("rids", reassigned)
                .ExecuteOnce(_cancellation);
        }

        public override Task DeleteOutgoing(Envelope[] envelopes)
        {
            return DatabaseSettings.CreateCommand(_deleteOutgoingEnvelopesSql)
                .With("ids", envelopes)
                .ExecuteOnce(_cancellation);
        }


        protected override string determineOutgoingEnvelopeSql(DatabaseSettings databaseSettings, AdvancedSettings settings)
        {
            return $"select body from {databaseSettings.SchemaName}.{DatabaseConstants.OutgoingTable} where owner_id = {TransportConstants.AnyNode} and destination = @destination LIMIT {settings.RecoveryBatchSize}";
        }

        public override Task Reassign(int ownerId, Envelope[] outgoing)
        {
            return _session.CreateCommand(_reassignSql)
                .With("owner", ownerId)
                .With("ids", outgoing)
                .ExecuteNonQueryAsync(_cancellation);
        }


        public override Task Delete(Envelope[] outgoing)
        {
            return _session.CreateCommand(_deleteOutgoingSql)
                .With("ids", outgoing)
                .ExecuteNonQueryAsync(_cancellation);
        }


    }
}
