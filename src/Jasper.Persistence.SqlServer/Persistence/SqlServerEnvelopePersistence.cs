using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Configuration;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Persistence.Database;
using Jasper.Persistence.SqlServer.Schema;
using Jasper.Persistence.SqlServer.Util;

namespace Jasper.Persistence.SqlServer.Persistence
{
    public class SqlServerEnvelopePersistence : DatabaseBackedEnvelopePersistence
    {
        private readonly SqlServerSettings _databaseSettings;


        public SqlServerEnvelopePersistence(SqlServerSettings databaseSettings, AdvancedSettings settings)
            : base(databaseSettings,settings, new SqlServerEnvelopeStorageAdmin(databaseSettings), new SqlServerDurabilityAgentStorage(databaseSettings, settings))
        {
            _databaseSettings = databaseSettings;
        }

        public override Task DeleteIncomingEnvelopes(Envelope[] envelopes)
        {
            return _databaseSettings.CallFunction("uspDeleteIncomingEnvelopes")
                .WithIdList(_databaseSettings, envelopes)
                .ExecuteOnce(_cancellation);
        }


        public override Task DeleteOutgoing(Envelope[] envelopes)
        {
            return _databaseSettings.CallFunction("uspDeleteOutgoingEnvelopes")
                .WithIdList(_databaseSettings, envelopes)
                .ExecuteOnce(_cancellation);
        }


        public override async Task MoveToDeadLetterStorage(ErrorReport[] errors)
        {
            var table = new DataTable();
            table.Columns.Add(new DataColumn("ID", typeof(Guid)));
            foreach (var error in errors) table.Rows.Add(error.Id);

            var cmd = new SqlCommand();
            var builder = new CommandBuilder(cmd);

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
                    $"insert into {_databaseSettings.SchemaName}.{DeadLetterTable} (id, source, message_type, explanation, exception_text, exception_type, exception_message, body) values (@{id.ParameterName}, @{source.ParameterName}, @{messageType.ParameterName}, @{explanation.ParameterName}, @{exText.ParameterName}, @{exType.ParameterName}, @{exMessage.ParameterName}, @{body.ParameterName});");
            }

            builder.Apply();

            using (var conn = new SqlConnection(_databaseSettings.ConnectionString))
            {
                await conn.OpenAsync(_cancellation);
                cmd.Connection = conn;
                await cmd.ExecuteNonQueryAsync(_cancellation);
            }
        }

        public override Task DiscardAndReassignOutgoing(Envelope[] discards, Envelope[] reassigned, int nodeId)
        {
            var cmd = _databaseSettings.CallFunction("uspDiscardAndReassignOutgoing")
                .WithIdList(_databaseSettings, discards, "discards")
                .WithIdList(_databaseSettings, reassigned, "reassigned")
                .With("ownerId", nodeId);

            return cmd.ExecuteOnce(_cancellation);
        }





        public override void Describe(TextWriter writer)
        {
            writer.WriteLine($"Sql Server Envelope Storage in Schema '{_databaseSettings.SchemaName}'");
        }








    }
}
