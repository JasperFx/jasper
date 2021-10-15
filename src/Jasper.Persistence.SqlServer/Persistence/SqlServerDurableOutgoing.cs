using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Persistence.Database;
using Jasper.Persistence.SqlServer.Util;
using Jasper.Transports;
using Jasper.Util;
using Weasel.Core;

namespace Jasper.Persistence.SqlServer.Persistence
{
    public class SqlServerDurableOutgoing : DurableOutgoing
    {
        private readonly IDatabaseSession _session;
        private readonly DatabaseSettings _settings;
        private readonly CancellationToken _cancellation;

        public SqlServerDurableOutgoing(IDatabaseSession session, DatabaseSettings settings, AdvancedSettings options) : base(session, settings, options)
        {
            _session = session;
            _settings = settings;
            _cancellation = options.Cancellation;
        }

        protected override string determineOutgoingEnvelopeSql(DatabaseSettings databaseSettings, AdvancedSettings settings)
        {
            return $"select top {settings.RecoveryBatchSize} body from {databaseSettings.SchemaName}.{DatabaseConstants.OutgoingTable} where owner_id = {TransportConstants.AnyNode} and destination = @destination";
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


    }
}
