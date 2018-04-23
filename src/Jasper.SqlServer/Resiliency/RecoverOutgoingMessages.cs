using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Configuration;
using Jasper.SqlServer.Persistence;
using Jasper.SqlServer.Util;
using Jasper.Util;

namespace Jasper.SqlServer.Resiliency
{
    public class RecoverOutgoingMessages : IMessagingAction
    {
        public static readonly int OutgoingMessageLockId = "recover-outgoing-messages".GetHashCode();
        private readonly IChannelGraph _channels;
        private readonly SqlServerSettings _mssqlSettings;
        private readonly ITransportLogger _logger;
        private readonly MessagingSettings _settings;
        private readonly string _findUniqueDestinations;
        private readonly string _findOutgoingEnvelopesSql;
        private readonly string _deleteOutgoingSql;

        public RecoverOutgoingMessages(IChannelGraph channels, MessagingSettings settings, SqlServerSettings mssqlSettings,
            ITransportLogger logger)
        {
            _channels = channels;
            _settings = settings;
            _mssqlSettings = mssqlSettings;
            _logger = logger;

            _findUniqueDestinations = $"select distinct destination from {_mssqlSettings.SchemaName}.{SqlServerEnvelopePersistor.OutgoingTable}";
            _findOutgoingEnvelopesSql = $"select body from {_mssqlSettings.SchemaName}.{SqlServerEnvelopePersistor.OutgoingTable} where owner_id = {TransportConstants.AnyNode} and destination = @destination limit {settings.Retries.RecoveryBatchSize}";
            _deleteOutgoingSql = $"delete from {_mssqlSettings.SchemaName}.{SqlServerEnvelopePersistor.OutgoingTable} where owner_id = :owner and destination = @destination";

        }

        public async Task<List<Uri>> FindAllOutgoingDestinations(SqlConnection conn)
        {
            var list = new List<Uri>();

            var cmd = conn.CreateCommand(_findUniqueDestinations);
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var text = await reader.GetFieldValueAsync<string>(0);
                    list.Add(text.ToUri());
                }
            }

            return list;
        }

        public async Task Execute(SqlConnection conn, ISchedulingAgent agent, SqlTransaction tx)
        {
            if (!await conn.TryGetGlobalTxLock(OutgoingMessageLockId))
                return;


            var destinations = await FindAllOutgoingDestinations(conn);

            var count = 0;
            foreach (var destination in destinations)
            {
                count += await recoverFrom(destination, conn, tx);
            }

            var wasMaxedOut = count >= _settings.Retries.RecoveryBatchSize;

            if (wasMaxedOut)
            {
                agent.RescheduleOutgoingRecovery();
            }
        }

        private async Task<int> recoverFrom(Uri destination, SqlConnection conn, SqlTransaction tx)
        {
            try
            {
                var channel = _channels.GetOrBuildChannel(destination);

                if (channel.Latched) return 0;

                var outgoing = await conn.CreateCommand(_findOutgoingEnvelopesSql)
                    .With("destination", destination.ToString(), SqlDbType.VarChar)
                    .ExecuteToEnvelopes();

                var filtered = filterExpired(conn, outgoing);

                // Might easily try to do this in the time between starting
                // and having the data fetched. Was able to make that happen in
                // (contrived) testing
                if (channel.Latched || !filtered.Any()) return 0;

                await markOwnership(conn, filtered);


                tx.Commit();

                _logger.RecoveredOutgoing(filtered);

                foreach (var envelope in filtered)
                {
                    try
                    {
                        await channel.QuickSend(envelope);
                    }
                    catch (Exception e)
                    {
                        _logger.LogException(e, message:$"Unable to enqueue {envelope} for sending");
                    }
                }

                return outgoing.Count();

            }
            catch (UnknownTransportException e)
            {
                _logger.LogException(e, message: $"Could not resolve a channel for {destination}");

                await DeleteFromOutgoingEnvelopes(conn, TransportConstants.AnyNode, destination);
                tx.Commit();

                return 0;
            }
        }

        private async Task markOwnership(SqlConnection conn, Envelope[] outgoing)
        {
            var cmd = conn.CreateCommand($"{_mssqlSettings.SchemaName}.uspMarkOutgoingOwnership");
            cmd.CommandType = CommandType.StoredProcedure;
            var list = cmd.Parameters.AddWithValue("IDLIST", SqlServerEnvelopePersistor.BuildIdTable(outgoing));
            list.SqlDbType = SqlDbType.Structured;
            list.TypeName = $"{_mssqlSettings.SchemaName}.EnvelopeIdList";
            cmd.Parameters.AddWithValue("owner", _settings.UniqueNodeId).SqlDbType = SqlDbType.Int;

            await cmd.ExecuteNonQueryAsync();
        }

        public Task DeleteFromOutgoingEnvelopes(SqlConnection conn, int ownerId, Uri destination)
        {
            return conn.CreateCommand(_deleteOutgoingSql)
                .With("destination", destination.ToString(), SqlDbType.VarChar)
                .With("owner", ownerId, SqlDbType.Int).ExecuteNonQueryAsync();
        }


        private Envelope[] filterExpired(SqlConnection conn, IEnumerable<Envelope> outgoing)
        {
            var expiredMessages = outgoing.Where(x => x.IsExpired()).ToArray();
            _logger.DiscardedExpired(expiredMessages);

            var cmd = conn.CreateCommand($"{_mssqlSettings.SchemaName}.uspDeleteOutgoingEnvelopes");
            cmd.CommandType = CommandType.StoredProcedure;
            var list = cmd.Parameters.AddWithValue("IDLIST", SqlServerEnvelopePersistor.BuildIdTable(expiredMessages));
            list.SqlDbType = SqlDbType.Structured;
            list.TypeName = $"{_mssqlSettings.SchemaName}.EnvelopeIdList";

            return outgoing.Where(x => !x.IsExpired()).ToArray();
        }
    }

}
