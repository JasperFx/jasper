using System.Threading;
using System.Threading.Tasks;
using Jasper.Persistence.Database;
using Jasper.Persistence.SqlServer.Util;
using Jasper.Transports;
using Weasel.Core;

namespace Jasper.Persistence.SqlServer.Persistence
{
    public class SqlServerDurableOutgoing : DurableOutgoing
    {
        private readonly CancellationToken _cancellation;
        private readonly IDatabaseSession _session;
        private readonly DatabaseSettings _settings;

        public SqlServerDurableOutgoing(IDatabaseSession session, DatabaseSettings settings, AdvancedSettings options) :
            base(session, settings, options)
        {
            _session = session;
            _settings = settings;
            _cancellation = options.Cancellation;
        }

        protected override string determineOutgoingEnvelopeSql(DatabaseSettings databaseSettings,
            AdvancedSettings settings)
        {
            return
                $"select top {settings.RecoveryBatchSize} body from {databaseSettings.SchemaName}.{DatabaseConstants.OutgoingTable} where owner_id = {TransportConstants.AnyNode} and destination = @destination";
        }

        public override Task Reassign(int ownerId, Envelope[] outgoing)
        {
            var cmd = _session.CallFunction("uspMarkOutgoingOwnership")
                .WithIdList(_settings, outgoing)
                .With("owner", ownerId);

            return cmd.ExecuteNonQueryAsync(_cancellation);
        }

        public override Task Delete(Envelope[] outgoing)
        {
            return _session
                .CallFunction("uspDeleteOutgoingEnvelopes")
                .WithIdList(_settings, outgoing)
                .ExecuteNonQueryAsync(_cancellation);
        }

        public override Task DiscardAndReassignOutgoing(Envelope[] discards, Envelope[] reassigned, int nodeId)
        {
            var cmd = DatabaseSettings.CallFunction("uspDiscardAndReassignOutgoing")
                .WithIdList(DatabaseSettings, discards, "discards")
                .WithIdList(DatabaseSettings, reassigned, "reassigned")
                .With("ownerId", nodeId);

            return cmd.ExecuteOnce(_cancellation);
        }

        public override Task DeleteOutgoing(Envelope[] envelopes)
        {
            return DatabaseSettings.CallFunction("uspDeleteOutgoingEnvelopes")
                .WithIdList(DatabaseSettings, envelopes).ExecuteOnce(_cancellation);
        }
    }
}
