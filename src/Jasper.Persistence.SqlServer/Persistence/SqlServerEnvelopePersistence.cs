using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
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
        private readonly SqlServerSettings _settings;


        public SqlServerEnvelopePersistence(SqlServerSettings settings, JasperOptions options)
            : base(settings, options, new SqlServerEnvelopeStorageAdmin(settings), new SqlServerDurabilityAgentStorage(settings, options))
        {
            _settings = settings;
        }

        public override Task DeleteIncomingEnvelopes(Envelope[] envelopes)
        {
            return _settings.CallFunction("uspDeleteIncomingEnvelopes")
                .WithIdList(_settings, envelopes)
                .ExecuteOnce(_cancellation);
        }


        public override Task DeleteOutgoing(Envelope[] envelopes)
        {
            return _settings.CallFunction("uspDeleteOutgoingEnvelopes")
                .WithIdList(_settings, envelopes)
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
            list.TypeName = $"{_settings.SchemaName}.EnvelopeIdList";

            builder.Append($"EXEC {_settings.SchemaName}.uspDeleteIncomingEnvelopes @IDLIST;");

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
                    $"insert into {_settings.SchemaName}.{DeadLetterTable} (id, source, message_type, explanation, exception_text, exception_type, exception_message, body) values (@{id.ParameterName}, @{source.ParameterName}, @{messageType.ParameterName}, @{explanation.ParameterName}, @{exText.ParameterName}, @{exType.ParameterName}, @{exMessage.ParameterName}, @{body.ParameterName});");
            }

            builder.Apply();

            using (var conn = new SqlConnection(_settings.ConnectionString))
            {
                await conn.OpenAsync(_cancellation);
                cmd.Connection = conn;
                await cmd.ExecuteNonQueryAsync(_cancellation);
            }
        }

        public override Task DiscardAndReassignOutgoing(Envelope[] discards, Envelope[] reassigned, int nodeId)
        {
            var cmd = _settings.CallFunction("uspDiscardAndReassignOutgoing")
                .WithIdList(_settings, discards, "discards")
                .WithIdList(_settings, reassigned, "reassigned")
                .With("ownerId", nodeId);

            return cmd.ExecuteOnce(_cancellation);
        }





        public override void Describe(TextWriter writer)
        {
            writer.WriteLine($"Sql Server Envelope Storage in Schema '{_settings.SchemaName}'");
        }








    }
}
