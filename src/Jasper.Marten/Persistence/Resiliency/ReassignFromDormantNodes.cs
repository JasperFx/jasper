using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Transports.Configuration;
using Marten;
using Marten.Util;

namespace Jasper.Marten.Persistence.Resiliency
{
    public class ReassignFromDormantNodes : IMessagingAction
    {
        private readonly EnvelopeTables _marker;
        public readonly int ReassignmentLockId = "jasper-reassign-envelopes".GetHashCode();
        private readonly string _reassignDormantNodeSql;

        public ReassignFromDormantNodes(EnvelopeTables marker, MessagingSettings settings)
        {
            _marker = marker;

            _reassignDormantNodeSql = $@"
update {marker.Incoming}
  set owner_id = 0
where
  owner_id in (
    select distinct owner_id from {marker.Incoming}
    where owner_id != 0 AND owner_id != {settings.UniqueNodeId} AND pg_try_advisory_xact_lock(owner_id)
  );

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

            await session.Connection.CreateCommand()
                .Sql(_reassignDormantNodeSql).ExecuteNonQueryAsync();

            await session.SaveChangesAsync();
        }
    }
}
