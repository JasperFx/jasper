using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.SqlServer.Util;

namespace Jasper.SqlServer.Persistence
{
    public class SqlServerEnvelopePersistor : IEnvelopePersistor
    {
        public const string IncomingTable = "jasper_incoming_envelopes";
        public const string OutgoingTable = "jasper_outgoing_envelopes";
        public const string DeadLetterTable = "jasper_dead_letters";

        private readonly SqlServerSettings _settings;

        public SqlServerEnvelopePersistor(SqlServerSettings settings)
        {
            _settings = settings;
        }

        public List<Envelope> AllIncomingEnvelopes()
        {
            using (var conn = new SqlConnection(_settings.ConnectionString))
            {
                conn.Open();

                return conn
                    .CreateCommand($"select body, status, owner_id, execution_time, attempts from {_settings.SchemaName}.{IncomingTable}")
                    .LoadEnvelopes();

            }
        }

        public List<Envelope> AllOutgoingEnvelopes()
        {
            using (var conn = new SqlConnection(_settings.ConnectionString))
            {
                conn.Open();

                return conn
                    .CreateCommand($"select body, '{TransportConstants.Outgoing}', owner_id, NULL from {_settings.SchemaName}.{OutgoingTable}")
                    .LoadEnvelopes();

            }
        }

        public async Task DeleteIncomingEnvelopes(Envelope[] envelopes)
        {
            var table = BuildIdTable(envelopes);

            using (var conn = new SqlConnection(_settings.ConnectionString))
            {
                await conn.OpenAsync();

                var cmd = conn.CreateCommand($"{_settings.SchemaName}.uspDeleteIncomingEnvelopes");
                cmd.CommandType = CommandType.StoredProcedure;
                var list = cmd.Parameters.AddWithValue("IDLIST", table);
                list.SqlDbType = SqlDbType.Structured;
                list.TypeName = $"{_settings.SchemaName}.EnvelopeIdList";

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static DataTable BuildIdTable(Envelope[] envelopes)
        {
            var table = new DataTable();
            table.Columns.Add(new DataColumn("ID", typeof(Guid)));
            foreach (var envelope in envelopes)
            {
                table.Rows.Add(envelope.Id);
            }

            return table;
        }

        public async Task DeleteOutgoingEnvelopes(Envelope[] envelopes)
        {
            var table = BuildIdTable(envelopes);

            using (var conn = new SqlConnection(_settings.ConnectionString))
            {
                await conn.OpenAsync();

                var cmd = conn.CreateCommand($"{_settings.SchemaName}.uspDeleteOutgoingEnvelopes");
                cmd.CommandType = CommandType.StoredProcedure;

                var list = cmd.Parameters.AddWithValue("IDLIST", table);
                list.SqlDbType = SqlDbType.Structured;
                list.TypeName = $"{_settings.SchemaName}.EnvelopeIdList";

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteOutgoingEnvelope(Envelope envelope)
        {
            using (var conn = new SqlConnection(_settings.ConnectionString))
            {
                await conn.OpenAsync();

                await conn.CreateCommand($"delete from {_settings.SchemaName}.{OutgoingTable} where id = @id")
                    .With("id", envelope.Id, SqlDbType.UniqueIdentifier)
                    .ExecuteNonQueryAsync();
            }
        }

        public async Task MoveToDeadLetterStorage(ErrorReport[] errors)
        {
            var table = new DataTable();
            table.Columns.Add(new DataColumn("ID", typeof(Guid)));
            foreach (var error in errors)
            {
                table.Rows.Add(error.Id);
            }

            var cmd = new SqlCommand();
            var builder = new CommandBuilder(cmd);

            var list = builder.AddNamedParameter("IDLIST", table);
            list.SqlDbType = SqlDbType.Structured;
            list.TypeName = $"{_settings.SchemaName}.EnvelopeIdList";

            builder.Append($"EXEC {_settings.SchemaName}.uspDeleteIncomingEnvelopes @IDLIST;");

            foreach (var error in errors)
            {
                var id = builder.AddParameter(error.Id, SqlDbType.UniqueIdentifier);
                var source = builder.AddParameter(error.Source, SqlDbType.VarChar);
                var messageType = builder.AddParameter(error.MessageType, SqlDbType.VarChar);
                var explanation = builder.AddParameter(error.Explanation, SqlDbType.VarChar);
                var exText = builder.AddParameter(error.ExceptionText, SqlDbType.VarChar);
                var exType = builder.AddParameter(error.ExceptionType, SqlDbType.VarChar);
                var exMessage = builder.AddParameter(error.ExceptionMessage, SqlDbType.VarChar);
                var body = builder.AddParameter(error.RawData, SqlDbType.VarBinary);

                builder.Append($"insert into {_settings.SchemaName}.{DeadLetterTable} (id, source, message_type, explanation, exception_text, exception_type, exception_message, body) values (@{id.ParameterName}, @{source.ParameterName}, @{messageType.ParameterName}, @{explanation.ParameterName}, @{exText.ParameterName}, @{exType.ParameterName}, @{exMessage.ParameterName}, @{body.ParameterName});");
            }

            builder.Apply();

            using (var conn = new SqlConnection(_settings.ConnectionString))
            {
                await conn.OpenAsync();
                cmd.Connection = conn;
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task ScheduleExecution(Envelope[] envelopes)
        {
            var cmd = new SqlCommand();
            var builder = new CommandBuilder(cmd);

            foreach (var envelope in envelopes)
            {
                var id = builder.AddParameter(envelope.Id, SqlDbType.UniqueIdentifier);
                var time = builder.AddParameter(envelope.ExecutionTime.Value, SqlDbType.DateTimeOffset);
                var attempts = builder.AddParameter(envelope.Attempts, SqlDbType.Int);

                builder.Append($"update {_settings.SchemaName}.{IncomingTable} set execution_time = @{time.ParameterName}, status = \'{TransportConstants.Scheduled}\', attempts = @{attempts.ParameterName} where id = @{id.ParameterName};");
            }

            builder.Apply();


            using (var conn = new SqlConnection(_settings.ConnectionString))
            {
                await conn.OpenAsync();

                cmd.Connection = conn;
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<ErrorReport> LoadDeadLetterEnvelope(Guid id)
        {
            using (var conn = new SqlConnection(_settings.ConnectionString))
            {
                await conn.OpenAsync();

                var cmd = conn.CreateCommand($"select body, explanation, exception_text, exception_type, exception_message, source, message_type, id from {_settings.SchemaName}.{DeadLetterTable} where id = @id");
                cmd.AddNamedParameter("id", id, SqlDbType.UniqueIdentifier);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (!(await reader.ReadAsync())) return null;


                    var report = new ErrorReport
                    {
                        RawData = await reader.GetFieldValueAsync<byte[]>(0),
                        Explanation = await reader.GetFieldValueAsync<string>(1),
                        ExceptionText = await reader.GetFieldValueAsync<string>(2),
                        ExceptionType = await reader.GetFieldValueAsync<string>(3),
                        ExceptionMessage = await reader.GetFieldValueAsync<string>(4),
                        Source = await reader.GetFieldValueAsync<string>(5),
                        MessageType = await reader.GetFieldValueAsync<string>(6),
                        Id = await reader.GetFieldValueAsync<Guid>(7)
                    };

                    return report;
                }
            }
        }

        public async Task IncrementIncomingEnvelopeAttempts(Envelope envelope)
        {
            using (var conn = new SqlConnection(_settings.ConnectionString))
            {
                await conn.OpenAsync();

                await conn.CreateCommand(
                        $"update {_settings.SchemaName}.{IncomingTable} set attempts = @attempts where id = @id")
                    .With("attempts", envelope.Attempts, SqlDbType.Int)
                    .With("id", envelope.Id, SqlDbType.UniqueIdentifier)
                    .ExecuteNonQueryAsync();
            }
        }

        public async Task StoreIncoming(Envelope envelope)
        {
            using (var conn = new SqlConnection(_settings.ConnectionString))
            {
                await conn.OpenAsync();

                var cmd = conn.CreateCommand($@"
insert into {_settings.SchemaName}.{IncomingTable}
  (id, status, owner_id, execution_time, attempts, body)
values
  (@id, @status, @owner, @time, @attempts, @body);
");

                await cmd
                    .With("id", envelope.Id, SqlDbType.UniqueIdentifier)
                    .With("status", envelope.Status, SqlDbType.VarChar)
                    .With("owner", envelope.OwnerId, SqlDbType.Int)
                    .With("attempts", envelope.Attempts, SqlDbType.Int)
                    .With("time", envelope.ExecutionTime, SqlDbType.DateTimeOffset)
                    .With("body", envelope.Serialize(), SqlDbType.VarBinary)
                    .ExecuteNonQueryAsync();

            }
        }

        public async Task StoreIncoming(IEnumerable<Envelope> envelopes)
        {
            var cmd = BuildIncomingStorageCommand(envelopes, _settings);

            using (var conn = new SqlConnection(_settings.ConnectionString))
            {
                await conn.OpenAsync();

                cmd.Connection = conn;

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static SqlCommand BuildIncomingStorageCommand(IEnumerable<Envelope> envelopes, SqlServerSettings settings)
        {
            var cmd = new SqlCommand();
            var builder = new CommandBuilder(cmd);

            foreach (var envelope in envelopes)
            {
                var id = builder.AddParameter(envelope.Id, SqlDbType.UniqueIdentifier);
                var status = builder.AddParameter(envelope.Status, SqlDbType.VarChar);
                var owner = builder.AddParameter(envelope.OwnerId, SqlDbType.Int);
                var attempts = builder.AddParameter(envelope.Attempts, SqlDbType.Int);
                var time = builder.AddParameter(envelope.ExecutionTime, SqlDbType.DateTimeOffset);
                var body = builder.AddParameter(envelope.Serialize(), SqlDbType.VarBinary);


                builder.Append(
                    $"insert into {settings.SchemaName}.{IncomingTable} (id, status, owner_id, execution_time, attempts, body) values (@{id.ParameterName}, @{status.ParameterName}, @{owner.ParameterName}, @{time.ParameterName}, @{attempts.ParameterName}, @{body.ParameterName});");
            }

            builder.Apply();

            return cmd;
        }

        public async Task DiscardAndReassignOutgoing(Envelope[] discards, Envelope[] reassigned, int nodeId)
        {
            var discardTable = BuildIdTable(discards);
            var reassignedTable = BuildIdTable(reassigned);

            using (var conn = new SqlConnection(_settings.ConnectionString))
            {
                await conn.OpenAsync();

                await conn.CreateCommand(
                        $"EXEC {_settings.SchemaName}.uspDiscardAndReassignOutgoing @discards, @reassigned, @owner")
                    .With("discards", discardTable, SqlDbType.Structured)
                    .With("reassigned", discardTable, SqlDbType.Structured)
                    .With("owner", nodeId, SqlDbType.Int).ExecuteNonQueryAsync();


            }
        }

        public async Task StoreOutgoing(Envelope envelope, int ownerId)
        {
            using (var conn = new SqlConnection(_settings.ConnectionString))
            {
                await conn.OpenAsync();

                await conn.CreateCommand(
                        $"insert into {_settings.SchemaName}.{OutgoingTable} (id, owner_id, destination, deliver_by, body) values (@id, @owner, @destination, @deliverBy, @body)")
                    .With("id", envelope.Id, SqlDbType.UniqueIdentifier)
                    .With("owner", ownerId, SqlDbType.Int)
                    .With("destination", envelope.Destination.ToString(), SqlDbType.VarChar)
                    .With("deliverBy", envelope.DeliverBy, SqlDbType.DateTimeOffset)
                    .With("body", envelope.Serialize(), SqlDbType.VarBinary)
                    .ExecuteNonQueryAsync();
            }
        }

        public async Task StoreOutgoing(Envelope[] envelopes, int ownerId)
        {
            var cmd = BuildOutgoingStorageCommand(envelopes, ownerId, _settings);

            using (var conn = new SqlConnection(_settings.ConnectionString))
            {
                await conn.OpenAsync();

                cmd.Connection = conn;

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static SqlCommand BuildOutgoingStorageCommand(Envelope[] envelopes, int ownerId,
            SqlServerSettings settings)
        {
            var cmd = new SqlCommand();
            var builder = new CommandBuilder(cmd);

            builder.AddNamedParameter("owner", ownerId).SqlDbType = SqlDbType.Int;

            foreach (var envelope in envelopes)
            {
                var id = builder.AddParameter(envelope.Id, SqlDbType.UniqueIdentifier);
                var destination = builder.AddParameter(envelope.Destination.ToString(), SqlDbType.VarChar);
                var deliverBy = builder.AddParameter(envelope.DeliverBy, SqlDbType.DateTimeOffset);
                var body = builder.AddParameter(envelope.Serialize(), SqlDbType.VarBinary);

                builder.Append(
                    $"insert into {settings.SchemaName}.{OutgoingTable} (id, owner_id, destination, deliver_by, body) values (@{id.ParameterName}, @owner, @{destination.ParameterName}, @{deliverBy.ParameterName}, @{body.ParameterName});");
            }

            builder.Apply();
            return cmd;
        }
    }
}
