using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Transports.Configuration;
using Marten;
using Marten.Services;
using Marten.Util;
using Npgsql;

namespace Jasper.Persistence.Marten.Resiliency
{
    public class SqlCommandOperation : IStorageOperation
    {
        private readonly string _sql;

        public SqlCommandOperation(string sql)
        {
            _sql = sql.Trim();
            if (!_sql.EndsWith(";"))
            {
                _sql += ";";
            }
        }

        public void ConfigureCommand(CommandBuilder builder)
        {
            builder.Append(_sql);
        }

        public Type DocumentType { get; } = null;
    }

    public class ReassignFromDormantNodes : IMessagingAction
    {
        private readonly EnvelopeTables _marker;
        public readonly int ReassignmentLockId = "jasper-reassign-envelopes".GetHashCode();
        private readonly string _reassignDormantNodeIncomingSql;
        private readonly string _reassignDormantNodeOutgoingSql;

        public ReassignFromDormantNodes(EnvelopeTables marker, MessagingSettings settings)
        {
            _marker = marker;

            _reassignDormantNodeIncomingSql = $@"
update {marker.Incoming}
  set owner_id = 0
where
  owner_id in (
    select distinct owner_id from {marker.Incoming}
    where owner_id != 0 AND owner_id != {settings.UniqueNodeId} AND pg_try_advisory_xact_lock(owner_id)
  );

";

            _reassignDormantNodeOutgoingSql = $@"

update {marker.Outgoing}
  set owner_id = 0
where
  owner_id in (
    select distinct owner_id from {marker.Outgoing}
    where owner_id != 0 AND owner_id != {settings.UniqueNodeId} AND pg_try_advisory_xact_lock(owner_id)
  );
";
        }

        public async Task Execute(IDocumentSession session, ISchedulingAgent agent)
        {
            if (!await session.TryGetGlobalTxLock(ReassignmentLockId))
            {
                return;
            }

            session.QueueOperation(new SqlCommandOperation(_reassignDormantNodeIncomingSql));
            session.QueueOperation(new SqlCommandOperation(_reassignDormantNodeOutgoingSql));

            await session.SaveChangesAsync();
        }
    }
}
