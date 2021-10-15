using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Baseline;
using Jasper.Persistence.Database;
using Jasper.Persistence.SqlServer.Schema;
using Jasper.Persistence.SqlServer.Util;
using Microsoft.Data.SqlClient;
using Weasel.Core;
using Weasel.SqlServer;
using CommandExtensions = Weasel.Core.CommandExtensions;

namespace Jasper.Persistence.SqlServer.Persistence
{
    public class SqlServerEnvelopePersistence : DatabaseBackedEnvelopePersistence
    {
        private readonly SqlServerSettings _databaseSettings;


        public SqlServerEnvelopePersistence(SqlServerSettings databaseSettings, AdvancedSettings settings)
            : base(databaseSettings, settings, new SqlServerEnvelopeStorageAdmin(databaseSettings),
                new SqlServerDurabilityAgentStorage(databaseSettings, settings))
        {
            _databaseSettings = databaseSettings;
        }

        public override Task DeleteIncomingEnvelopes(Envelope[] envelopes)
        {
            return CommandExtensions.ExecuteOnce(_databaseSettings.CallFunction("uspDeleteIncomingEnvelopes")
                .WithIdList(_databaseSettings, envelopes), _cancellation);
        }


        public override Task DeleteOutgoing(Envelope[] envelopes)
        {
            return CommandExtensions.ExecuteOnce(_databaseSettings.CallFunction("uspDeleteOutgoingEnvelopes")
                .WithIdList(_databaseSettings, envelopes), _cancellation);
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

            await using (var conn = new SqlConnection(_databaseSettings.ConnectionString))
            {
                await conn.OpenAsync(_cancellation);
                await builder.ExecuteNonQueryAsync(conn);
            }
        }

        public override Task DiscardAndReassignOutgoing(Envelope[] discards, Envelope[] reassigned, int nodeId)
        {
            var cmd = _databaseSettings.CallFunction("uspDiscardAndReassignOutgoing")
                .WithIdList(_databaseSettings, discards, "discards")
                .WithIdList(_databaseSettings, reassigned, "reassigned")
                .With("ownerId", nodeId);

            return CommandExtensions.ExecuteOnce(cmd, _cancellation);
        }


        public override void Describe(TextWriter writer)
        {
            writer.WriteLine($"Sql Server Envelope Storage in Schema '{_databaseSettings.SchemaName}'");
        }
    }
}
