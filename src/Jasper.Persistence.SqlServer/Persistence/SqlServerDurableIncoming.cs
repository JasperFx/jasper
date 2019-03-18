using System.Threading;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Persistence.Database;
using Jasper.Persistence.SqlServer.Util;

namespace Jasper.Persistence.SqlServer.Persistence
{
    public class SqlServerDurableIncoming : DataAccessor, IDurableIncoming
    {
        private readonly IDatabaseSession _session;
        private readonly SqlServerSettings _settings;
        private readonly string _findAtLargeEnvelopesSql;
        private readonly CancellationToken _cancellation;

        public SqlServerDurableIncoming(IDatabaseSession session, SqlServerSettings settings, JasperOptions options)
        {
            _session = session;
            _settings = settings;
            _findAtLargeEnvelopesSql =
                $"select top {options.Retries.RecoveryBatchSize} body from {settings.SchemaName}.{IncomingTable} where owner_id = {TransportConstants.AnyNode} and status = '{TransportConstants.Incoming}'";

            _cancellation = options.Cancellation;
        }

        public Task<Envelope[]> LoadPageOfLocallyOwned()
        {
            return _session.CreateCommand(_findAtLargeEnvelopesSql)
                .ExecuteToEnvelopes(_cancellation);
        }

        public Task Reassign(int ownerId, Envelope[] incoming)
        {
            return _session.CallFunction("uspMarkIncomingOwnership")
                .WithIdList(_settings, incoming)
                .With("owner", ownerId)
                .ExecuteNonQueryAsync(_cancellation);
        }
    }
}
