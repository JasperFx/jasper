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
    public class OwnershipMarker
    {
        private readonly string _markIncomingSql;
        private readonly string _markOutgoingSql;
        private readonly string _markAtLargeSql;

        public OwnershipMarker(BusSettings settings, StoreOptions storeConfiguration)
        {
            var dbObjectName = storeConfiguration.Storage.MappingFor(typeof(Envelope)).Table;
            _markIncomingSql = $"update {dbObjectName} set data = data || '{{\"{nameof(Envelope.Status)}\": \"{TransportConstants.Incoming}\", \"{nameof(Envelope.OwnerId)}\": \"{settings.UniqueNodeId}\"}}' where id = ANY(:idlist)";
            _markOutgoingSql = $"update {dbObjectName} set data = data || '{{\"{nameof(Envelope.Status)}\": \"{TransportConstants.Outgoing}\", \"{nameof(Envelope.OwnerId)}\": \"{settings.UniqueNodeId}\"}}' where id = ANY(:idlist)";
            _markAtLargeSql = $"update {dbObjectName} set data = data || '{{\"{nameof(Envelope.Status)}\": \"{TransportConstants.Outgoing}\", \"{nameof(Envelope.OwnerId)}\": \"{TransportConstants.AnyNode}\"}}' where id = ANY(:idlist)";



        }

        public Task MarkIncomingOwnedByThisNode(IDocumentSession session, params Envelope[] envelopes)
        {
            return execute(session, _markIncomingSql, envelopes);
        }

        public Task MarkOutgoingOwnedByThisNode(IDocumentSession session, params Envelope[] envelopes)
        {
            return execute(session, _markOutgoingSql, envelopes);
        }

        public Task MarkOwnedByAnyNode(IDocumentSession session, params Envelope[] envelopes)
        {
            return execute(session, _markAtLargeSql, envelopes);
        }

        private Task execute(IDocumentSession session, string sql, Envelope[] envelopes)
        {
            var identities = envelopes.Select(x => x.Id).ToArray();

            return session.Connection.CreateCommand()
                .Sql(sql)
                .With("idlist", identities, NpgsqlDbType.Array | NpgsqlDbType.Varchar)
                .ExecuteNonQueryAsync();
        }
    }
}