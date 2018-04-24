using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.WorkerQueues;
using Jasper.SqlServer.Persistence;
using Jasper.SqlServer.Util;

namespace Jasper.SqlServer.Resiliency
{

    public class RecoverIncomingMessages : IMessagingAction
    {
        public static readonly int IncomingMessageLockId = "recover-incoming-messages".GetHashCode();
        private readonly ITransportLogger _logger;
        private readonly SqlServerSettings _mssqlSettings;
        private readonly MessagingSettings _settings;
        private readonly IWorkerQueue _workers;
        private readonly string _findAtLargeEnvelopesSql;

        public RecoverIncomingMessages(IWorkerQueue workers, MessagingSettings settings, SqlServerSettings mssqlSettings,
            ITransportLogger logger)
        {
            _workers = workers;
            _settings = settings;
            _mssqlSettings = mssqlSettings;
            _logger = logger;

            _findAtLargeEnvelopesSql = $"select body from {mssqlSettings.SchemaName}.{SqlServerEnvelopePersistor.IncomingTable} where owner_id = {TransportConstants.AnyNode} and status = '{TransportConstants.Incoming}' limit {settings.Retries.RecoveryBatchSize}";
        }

        public async Task Execute(SqlConnection conn, ISchedulingAgent agent, SqlTransaction tx)
        {
            if (!await conn.TryGetGlobalTxLock(tx, IncomingMessageLockId))
                return;

            var incoming = await conn.CreateCommand(_findAtLargeEnvelopesSql)
                .ExecuteToEnvelopes();

            if (!incoming.Any()) return;

            await markOwnership(conn, incoming);

            tx.Commit();

            _logger.RecoveredIncoming(incoming);

            foreach (var envelope in incoming)
            {
                envelope.OwnerId = _settings.UniqueNodeId;
                await _workers.Enqueue(envelope);
            }

            if (incoming.Count == _settings.Retries.RecoveryBatchSize &&
                _workers.QueuedCount < _settings.MaximumLocalEnqueuedBackPressureThreshold)
            {
                agent.RescheduleIncomingRecovery();
            }
        }

        private async Task markOwnership(SqlConnection conn, List<Envelope> incoming)
        {
            var cmd = conn.CreateCommand($"{_mssqlSettings.SchemaName}.uspMarkIncomingOwnership");
            cmd.CommandType = CommandType.StoredProcedure;
            var list = cmd.Parameters.AddWithValue("IDLIST", SqlServerEnvelopePersistor.BuildIdTable(incoming));
            list.SqlDbType = SqlDbType.Structured;
            list.TypeName = $"{_mssqlSettings.SchemaName}.EnvelopeIdList";
            cmd.Parameters.AddWithValue("owner", _settings.UniqueNodeId).SqlDbType = SqlDbType.Int;

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
