using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Marten;
using Marten.Util;
using NpgsqlTypes;

namespace Jasper.Marten.Persistence.Resiliency
{
    // TODO -- use Marten operations instead of direct SQL calls here to batch up commands
    public class OwnershipMarker
    {
        private readonly string _reassignDormantNodeSql;
        private readonly string _markOwnerAndStatusSql;
        private readonly int _currentNodeId;
        private readonly string _markOwnedSql;

        public OwnershipMarker(BusSettings settings, StoreOptions storeConfiguration)
        {
            var dbObjectName = storeConfiguration.Storage.MappingFor(typeof(Envelope)).Table;
            _markOwnerAndStatusSql = $"update {dbObjectName} set status = :status, owner_id = :owner where id = ANY(:idlist)";
            _markOwnedSql = $"update {dbObjectName} set owner_id = :owner where id = ANY(:idlist)";

            _currentNodeId = settings.UniqueNodeId;

            _reassignDormantNodeSql = $@"
update {dbObjectName}
  set owner_id = 0
where
  owner_id in (
    select distinct owner_id from {dbObjectName}
    where owner_id != 0 AND owner_id != {settings.UniqueNodeId} AND pg_try_advisory_xact_lock(owner_id)
  )
";
        }

        public Task MarkIncomingOwnedByThisNode(IDocumentSession session, params Envelope[] envelopes)
        {
            return execute(session, _currentNodeId, TransportConstants.Incoming, envelopes);
        }

        public Task MarkOutgoingOwnedByThisNode(IDocumentSession session, params Envelope[] envelopes)
        {
            return execute(session, _currentNodeId, TransportConstants.Outgoing, envelopes);
        }

        public Task MarkOwnedByAnyNode(IDocumentSession session, params Envelope[] envelopes)
        {
            var identities = envelopes.Select(x => x.Id).ToArray();

            return session.Connection.CreateCommand()
                .Sql(_markOwnedSql)
                .With("idlist", identities, NpgsqlDbType.Array | NpgsqlDbType.Varchar)
                .With("owner", TransportConstants.AnyNode, NpgsqlDbType.Integer)
                .ExecuteNonQueryAsync();
        }


        private Task execute(IDocumentSession session, int owner, string status, Envelope[] envelopes)
        {
            var identities = envelopes.Select(x => x.Id).ToArray();

            return session.Connection.CreateCommand()
                .Sql(_markOwnerAndStatusSql)
                .With("idlist", identities, NpgsqlDbType.Array | NpgsqlDbType.Varchar)
                .With("status", status, NpgsqlDbType.Varchar)
                .With("owner", owner, NpgsqlDbType.Integer)
                .ExecuteNonQueryAsync();
        }

        public Task ReassignEnvelopesFromDormantNodes(IDocumentSession session)
        {
            return session.Connection.CreateCommand()
                .Sql(_reassignDormantNodeSql).ExecuteNonQueryAsync();

        }
    }
}
