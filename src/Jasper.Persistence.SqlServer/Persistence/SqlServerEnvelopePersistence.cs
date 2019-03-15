using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Persistence.SqlServer.Schema;
using Jasper.Persistence.SqlServer.Util;

namespace Jasper.Persistence.SqlServer.Persistence
{
    public class SqlServerEnvelopePersistence : SqlServerAccess, IEnvelopePersistence
    {


        private readonly SqlServerSettings _settings;
        private readonly JasperOptions _options;
        private readonly CancellationToken _cancellation;
        private readonly string _incrementIncomingAttempts;
        private readonly string _storeIncoming;
        private readonly string _insertOutgoingSql;

        public SqlServerEnvelopePersistence(SqlServerSettings settings, JasperOptions options)
        {
            _settings = settings;
            Admin = new SqlServerEnvelopeStorageAdmin(settings);

            AgentStorage = new SqlServerDurabilityAgentStorage(settings, options);

            _options = options;
            _cancellation = options.Cancellation;

            _incrementIncomingAttempts = $"update {_settings.SchemaName}.{IncomingTable} set attempts = @attempts where id = @id";
            _storeIncoming = $@"
insert into {_settings.SchemaName}.{IncomingTable}
  (id, status, owner_id, execution_time, attempts, body)
values
  (@id, @status, @owner, @time, @attempts, @body);
";
            _insertOutgoingSql = $"insert into {_settings.SchemaName}.{OutgoingTable} (id, owner_id, destination, deliver_by, body) values (@id, @owner, @destination, @deliverBy, @body)";
        }

        public IEnvelopeStorageAdmin Admin { get; }
        public IDurabilityAgentStorage AgentStorage { get; }

        public Task DeleteIncomingEnvelopes(Envelope[] envelopes)
        {
            return _settings.CallFunction("uspDeleteIncomingEnvelopes")
                .WithIdList(_settings, envelopes)
                .ExecuteOnce(_cancellation);
        }

        public Task DeleteIncomingEnvelope(Envelope envelope)
        {
            return _settings.CreateCommand($"delete from {_settings.SchemaName}.{IncomingTable} where id = @id")
                .With("id", envelope.Id)
                .ExecuteOnce(_cancellation);
        }

        public Task DeleteOutgoing(Envelope[] envelopes)
        {
            return _settings.CallFunction("uspDeleteOutgoingEnvelopes")
                .WithIdList(_settings, envelopes)
                .ExecuteOnce(_cancellation);
        }

        public Task DeleteOutgoing(Envelope envelope)
        {
            return _settings.CreateCommand($"delete from {_settings.SchemaName}.{OutgoingTable} where id = @id")
                .With("id", envelope.Id)
                .ExecuteOnce(_cancellation);

        }

        public async Task MoveToDeadLetterStorage(ErrorReport[] errors)
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

        public async Task ScheduleExecution(Envelope[] envelopes)
        {
            var cmd = new SqlCommand();
            var builder = new CommandBuilder(cmd);

            foreach (var envelope in envelopes)
            {
                var id = builder.AddParameter(envelope.Id);
                var time = builder.AddParameter(envelope.ExecutionTime.Value);
                var attempts = builder.AddParameter(envelope.Attempts);

                builder.Append(
                    $"update {_settings.SchemaName}.{IncomingTable} set execution_time = @{time.ParameterName}, status = \'{TransportConstants.Scheduled}\', attempts = @{attempts.ParameterName}, owner_id = {TransportConstants.AnyNode} where id = @{id.ParameterName};");
            }

            builder.Apply();


            using (var conn = new SqlConnection(_settings.ConnectionString))
            {
                await conn.OpenAsync(_cancellation);

                cmd.Connection = conn;
                await cmd.ExecuteNonQueryAsync(_cancellation);
            }
        }


        public Task IncrementIncomingEnvelopeAttempts(Envelope envelope)
        {
            return _settings.CreateCommand(_incrementIncomingAttempts)
                .With("attempts", envelope.Attempts)
                .With("id", envelope.Id)
                .ExecuteOnce(_cancellation);
        }

        public Task StoreIncoming(Envelope envelope)
        {
            return _settings.CreateCommand(_storeIncoming)
                .With("id", envelope.Id)
                .With("status", envelope.Status)
                .With("owner", envelope.OwnerId)
                .With("attempts", envelope.Attempts)
                .With("time", envelope.ExecutionTime)
                .With("body", envelope.Serialize())
                .ExecuteOnce(_cancellation);
        }

        public Task StoreIncoming(SqlTransaction tx, Envelope[] envelopes)
        {
            var cmd = BuildIncomingStorageCommand(envelopes, _settings);

            cmd.Transaction = tx;
            cmd.Connection = tx.Connection;

            return cmd.ExecuteNonQueryAsync(_cancellation);
        }

        public async Task StoreIncoming(Envelope[] envelopes)
        {
            var cmd = BuildIncomingStorageCommand(envelopes, _settings);

            using (var conn = new SqlConnection(_settings.ConnectionString))
            {
                await conn.OpenAsync(_cancellation);

                cmd.Connection = conn;

                await cmd.ExecuteNonQueryAsync(_cancellation);
            }
        }

        public Task DiscardAndReassignOutgoing(Envelope[] discards, Envelope[] reassigned, int nodeId)
        {
            var cmd = _settings.CallFunction("uspDiscardAndReassignOutgoing")
                .WithIdList(_settings, discards, "discards")
                .WithIdList(_settings, reassigned, "reassigned")
                .With("ownerId", nodeId);

            return cmd.ExecuteOnce(_cancellation);
        }



        public Task StoreOutgoing(Envelope envelope, int ownerId)
        {
            envelope.EnsureData();

            return _settings.CreateCommand(_insertOutgoingSql)
                .With("id", envelope.Id)
                .With("owner", ownerId)
                .With("destination", envelope.Destination.ToString())
                .With("deliverBy", envelope.DeliverBy)
                .With("body", envelope.Serialize())
                .ExecuteOnce(_cancellation);
        }

        public Task StoreOutgoing(SqlTransaction tx, Envelope[] envelopes)
        {
            var cmd = BuildOutgoingStorageCommand(envelopes, _options.UniqueNodeId, _settings);
            cmd.Connection = tx.Connection;
            cmd.Transaction = tx;

            return cmd.ExecuteNonQueryAsync(_cancellation);
        }

        public async Task StoreOutgoing(Envelope[] envelopes, int ownerId)
        {
            var cmd = BuildOutgoingStorageCommand(envelopes, ownerId, _settings);

            using (var conn = new SqlConnection(_settings.ConnectionString))
            {
                await conn.OpenAsync(_cancellation);

                cmd.Connection = conn;

                await cmd.ExecuteNonQueryAsync(_cancellation);
            }
        }


        public void Describe(TextWriter writer)
        {
            writer.WriteLine($"Sql Server Envelope Storage in Schema '{_settings.SchemaName}'");
        }

        public Task ScheduleJob(Envelope envelope)
        {
            envelope.Status = TransportConstants.Scheduled;
            envelope.OwnerId = TransportConstants.AnyNode;

            // TODO -- will be special latewr
            return StoreIncoming(envelope);
        }

        public Envelope[] AllIncomingEnvelopes()
        {
            using (var conn = new SqlConnection(_settings.ConnectionString))
            {
                conn.Open();

                return conn
                    .CreateCommand(
                        $"select body, status, owner_id, execution_time, attempts from {_settings.SchemaName}.{IncomingTable}")
                    .LoadEnvelopes();
            }
        }

        public Envelope[] AllOutgoingEnvelopes()
        {
            using (var conn = new SqlConnection(_settings.ConnectionString))
            {
                conn.Open();

                return conn
                    .CreateCommand(
                        $"select body, '{TransportConstants.Outgoing}', owner_id, NULL from {_settings.SchemaName}.{OutgoingTable}")
                    .LoadEnvelopes();
            }
        }



        public static SqlCommand BuildIncomingStorageCommand(IEnumerable<Envelope> envelopes,
            SqlServerSettings settings)
        {
            var cmd = new SqlCommand();
            var builder = new CommandBuilder(cmd);

            foreach (var envelope in envelopes)
            {
                envelope.EnsureData();

                var id = builder.AddParameter(envelope.Id);
                var status = builder.AddParameter(envelope.Status);
                var owner = builder.AddParameter(envelope.OwnerId);
                var attempts = builder.AddParameter(envelope.Attempts);
                var time = builder.AddParameter(envelope.ExecutionTime);
                var body = builder.AddParameter(envelope.Serialize());


                builder.Append(
                    $"insert into {settings.SchemaName}.{IncomingTable} (id, status, owner_id, execution_time, attempts, body) values (@{id.ParameterName}, @{status.ParameterName}, @{owner.ParameterName}, @{time.ParameterName}, @{attempts.ParameterName}, @{body.ParameterName});");
            }

            builder.Apply();

            return cmd;
        }

        public static SqlCommand BuildOutgoingStorageCommand(Envelope[] envelopes, int ownerId,
            SqlServerSettings settings)
        {
            var cmd = new SqlCommand();
            var builder = new CommandBuilder(cmd);

            builder.AddNamedParameter("owner", ownerId).DbType = DbType.Int32;

            foreach (var envelope in envelopes)
            {
                envelope.EnsureData();

                var id = builder.AddParameter(envelope.Id);
                var destination = builder.AddParameter(envelope.Destination.ToString());
                var deliverBy = builder.AddParameter(envelope.DeliverBy);
                var body = builder.AddParameter(envelope.Serialize());

                builder.Append(
                    $"insert into {settings.SchemaName}.{OutgoingTable} (id, owner_id, destination, deliver_by, body) values (@{id.ParameterName}, @owner, @{destination.ParameterName}, @{deliverBy.ParameterName}, @{body.ParameterName});");
            }

            builder.Apply();
            return cmd;
        }

        public void ClearAllStoredMessages()
        {
            _settings
                .ExecuteSql(
                    $"delete from {_settings.SchemaName}.{IncomingTable};delete from {_settings.SchemaName}.{OutgoingTable};delete from {_settings.SchemaName}.{DeadLetterTable}");

        }




    }
}
