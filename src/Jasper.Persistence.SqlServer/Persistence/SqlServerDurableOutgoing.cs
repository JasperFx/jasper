using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Persistence.Database;
using Jasper.Persistence.SqlServer.Util;
using Jasper.Util;

namespace Jasper.Persistence.SqlServer.Persistence
{
    public class SqlServerDurableOutgoing : DurableOutgoing
    {
        private readonly IDatabaseSession _session;
        private readonly DatabaseSettings _settings;
        private readonly CancellationToken _cancellation;

        public SqlServerDurableOutgoing(IDatabaseSession session, DatabaseSettings settings, JasperOptions options) : base(session, settings, options)
        {
            _session = session;
            _settings = settings;
            _cancellation = options.Cancellation;
        }

        protected override string determineOutgoingEnvelopeSql(Database.DatabaseSettings settings, JasperOptions options)
        {
            return $"select top {options.Advanced.RecoveryBatchSize} body from {settings.SchemaName}.{OutgoingTable} where owner_id = {TransportConstants.AnyNode} and destination = @destination";
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
