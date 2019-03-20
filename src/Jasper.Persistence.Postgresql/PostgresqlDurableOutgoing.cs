using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Persistence.Database;
using Jasper.Persistence.Postgresql.Util;
using Jasper.Util;
using NpgsqlTypes;

namespace Jasper.Persistence.Postgresql
{
    public class PostgresqlDurableOutgoing : DurableOutgoing
    {
        private readonly DurableStorageSession _session;
        private readonly string _deleteOutgoingSql;
        private readonly string _reassignSql;
        private readonly CancellationToken _cancellation;

        public PostgresqlDurableOutgoing(DurableStorageSession session, DatabaseSettings settings, JasperOptions options)
            : base(session, settings, options)
        {
            _session = session;

            _deleteOutgoingSql =
                $"delete from {settings.SchemaName}.{OutgoingTable} where id = ANY(@ids)";

            _reassignSql = $"update {settings.SchemaName}.{OutgoingTable} set owner_id = @owner where id = ANY(@ids)";

            _cancellation = options.Cancellation;
        }


        protected override string determineOutgoingEnvelopeSql(DatabaseSettings settings, JasperOptions options)
        {
            return $"select body from {settings.SchemaName}.{OutgoingTable} where owner_id = {TransportConstants.AnyNode} and destination = @destination LIMIT {options.Retries.RecoveryBatchSize}";
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
