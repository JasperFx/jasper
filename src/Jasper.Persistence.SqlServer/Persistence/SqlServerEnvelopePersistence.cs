using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Baseline;
using Jasper.Persistence.Database;
using Jasper.Persistence.Durability;
using Jasper.Persistence.SqlServer.Schema;
using Jasper.Persistence.SqlServer.Util;
using Jasper.Transports;
using Microsoft.Data.SqlClient;
using Weasel.Core;
using Weasel.SqlServer;
using CommandExtensions = Weasel.Core.CommandExtensions;

namespace Jasper.Persistence.SqlServer.Persistence
{
    public class SqlServerEnvelopePersistence : DatabaseBackedEnvelopePersistence
    {
        private readonly SqlServerSettings _databaseSettings;
        private readonly string _findAtLargeEnvelopesSql;


        public SqlServerEnvelopePersistence(SqlServerSettings databaseSettings, AdvancedSettings settings)
            : base(databaseSettings, settings, new SqlServerEnvelopeStorageAdmin(databaseSettings))
        {
            _databaseSettings = databaseSettings;
            _findAtLargeEnvelopesSql =
                $"select top {settings.RecoveryBatchSize} body, attempts from {databaseSettings.SchemaName}.{DatabaseConstants.IncomingTable} where owner_id = {TransportConstants.AnyNode} and status = '{EnvelopeStatus.Incoming}'";

        }

        public override Task DeleteIncomingEnvelopes(Envelope[] envelopes)
        {
            return _databaseSettings.CallFunction("uspDeleteIncomingEnvelopes")
                .WithIdList(_databaseSettings, envelopes).ExecuteOnce(_cancellation);
        }

        public override async Task MoveToDeadLetterStorage(ErrorReport[] errors)
        {
            var table = new DataTable();
            table.Columns.Add(new DataColumn("ID", typeof(Guid)));
            foreach (var error in errors) table.Rows.Add(error.Id);

            var builder = new CommandBuilder();

            var list = builder.AddNamedParameter("IDLIST", table).As<SqlParameter>();
            list.SqlDbType = SqlDbType.Structured;
            list.TypeName = $"{_databaseSettings.SchemaName}.EnvelopeIdList";

            builder.Append($"EXEC {_databaseSettings.SchemaName}.uspDeleteIncomingEnvelopes @IDLIST;");

            foreach (var error in errors)
            {
                var id = builder.AddParameter(error.Id);
                var source = builder.AddParameter(error.Source);
                var messageType = builder.AddParameter(error.MessageType);
                var explanation = builder.AddParameter(error.Explanation);
                var exText = builder.AddParameter(error.ExceptionText);
                var exType = builder.AddParameter(error.ExceptionType);
                var exMessage = builder.AddParameter(error.ExceptionMessage);
                var body = builder.AddParameter(error.RawData);

                builder.Append(
                    $"insert into {_databaseSettings.SchemaName}.{DatabaseConstants.DeadLetterTable} (id, source, message_type, explanation, exception_text, exception_type, exception_message, body) values (@{id.ParameterName}, @{source.ParameterName}, @{messageType.ParameterName}, @{explanation.ParameterName}, @{exText.ParameterName}, @{exType.ParameterName}, @{exMessage.ParameterName}, @{body.ParameterName});");
            }

            await using var conn = new SqlConnection(_databaseSettings.ConnectionString);
            await conn.OpenAsync(_cancellation);
            await builder.ExecuteNonQueryAsync(conn);
        }

        public override void Describe(TextWriter writer)
        {
            writer.WriteLine($"Sql Server Envelope Storage in Schema '{_databaseSettings.SchemaName}'");
        }

        protected override string determineOutgoingEnvelopeSql(DatabaseSettings databaseSettings,
            AdvancedSettings settings)
        {
            return
                $"select top {settings.RecoveryBatchSize} body from {databaseSettings.SchemaName}.{DatabaseConstants.OutgoingTable} where owner_id = {TransportConstants.AnyNode} and destination = @destination";
        }

        public override Task ReassignOutgoing(int ownerId, Envelope[] outgoing)
        {
            var cmd = Session.CallFunction("uspMarkOutgoingOwnership")
                .WithIdList(DatabaseSettings, outgoing)
                .With("owner", ownerId);

            return cmd.ExecuteNonQueryAsync(_cancellation);
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

        public override Task<Envelope[]> LoadPageOfLocallyOwnedIncoming()
        {
            return Session.CreateCommand(_findAtLargeEnvelopesSql)
                .ExecuteToEnvelopesWithAttempts(_cancellation);
        }

        public override Task ReassignIncoming(int ownerId, Envelope[] incoming)
        {
            return Session.CallFunction("uspMarkIncomingOwnership")
                .WithIdList(_databaseSettings, incoming)
                .With("owner", ownerId)
                .ExecuteNonQueryAsync(_cancellation);
        }
    }
}
