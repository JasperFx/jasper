using System.Threading;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Persistence.Database;
using Jasper.Persistence.Durability;
using Jasper.Persistence.SqlServer.Util;
using Jasper.Transports;

namespace Jasper.Persistence.SqlServer.Persistence
{
    public class SqlServerDurableIncoming : DataAccessor, IDurableIncoming
    {
        private readonly IDatabaseSession _session;
        private readonly DatabaseSettings _databaseSettings;
        private readonly string _findAtLargeEnvelopesSql;
        private readonly CancellationToken _cancellation;

        public SqlServerDurableIncoming(IDatabaseSession session, DatabaseSettings databaseSettings, AdvancedSettings settings)
        {
            _session = session;
            _databaseSettings = databaseSettings;
            _findAtLargeEnvelopesSql =
                $"select top {settings.RecoveryBatchSize} body from {databaseSettings.SchemaName}.{IncomingTable} where owner_id = {TransportConstants.AnyNode} and status = '{EnvelopeStatus.Incoming}'";

            _cancellation = settings.Cancellation;
        }

        public Task<Envelope[]> LoadPageOfLocallyOwned()
        {
            return _session.CreateCommand(_findAtLargeEnvelopesSql)
                .ExecuteToEnvelopes(_cancellation);
        }

        public Task Reassign(int ownerId, Envelope[] incoming)
        {
            return _session.CallFunction("uspMarkIncomingOwnership")
                .WithIdList(_databaseSettings, incoming)
                .With("owner", ownerId)
                .ExecuteNonQueryAsync(_cancellation);
        }
    }
}
