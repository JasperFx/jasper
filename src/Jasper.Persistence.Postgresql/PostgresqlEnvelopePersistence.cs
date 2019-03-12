using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Persistence.Postgresql.Schema;
using Jasper.Persistence.Postgresql.Util;
using Npgsql;
using NpgsqlTypes;

namespace Jasper.Persistence.Postgresql
{
    public class PostgresqlEnvelopePersistence : PostgresqlAccess,IEnvelopePersistence
    {
        private readonly string _deleteIncomingEnvelopesSql;
        private readonly string _deleteOutgoingEnvelopesSql;
        private readonly string _deleteIncomingEnvelopeSql;
        private readonly string _deleteOutgoingEnvelopeSql;

        public PostgresqlEnvelopePersistence(PostgresqlSettings settings, JasperOptions options)
        {
            Settings = settings;
            Options = options;

            Admin = new PostgresqlEnvelopeStorageAdmin(settings.ConnectionString, settings.SchemaName);
            _deleteIncomingEnvelopesSql = $"delete from {Settings.SchemaName}.{IncomingTable} WHERE id = ANY(:ids);";
            _deleteOutgoingEnvelopesSql = $"delete from {Settings.SchemaName}.{OutgoingTable} WHERE id = ANY(:ids);";

            _deleteIncomingEnvelopeSql = $"delete from {Settings.SchemaName}.{IncomingTable} WHERE id = :id;";
            _deleteOutgoingEnvelopeSql = $"delete from {Settings.SchemaName}.{OutgoingTable} WHERE id = :id;";

            AgentStorage = new PostgresqlDurabilityAgentStorage(settings, options);
        }

        public PostgresqlSettings Settings { get; }
        public JasperOptions Options { get; }

        public IEnvelopeStorageAdmin Admin { get; }
        public IDurabilityAgentStorage AgentStorage { get; }

        public async Task DeleteIncomingEnvelopes(Envelope[] envelopes)
        {
            using (var conn = new NpgsqlConnection(Settings.ConnectionString))
            {
                await conn.OpenAsync();

                await conn.CreateCommand(_deleteIncomingEnvelopesSql)
                    .With("ids", envelopes.Select(x => x.Id).ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Uuid)
                    .ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteIncomingEnvelope(Envelope envelope)
        {
            using (var conn = new NpgsqlConnection(Settings.ConnectionString))
            {
                await conn.OpenAsync();

                await conn.CreateCommand(_deleteIncomingEnvelopeSql)
                    .With("id", envelope.Id, NpgsqlDbType.Uuid)
                    .ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteOutgoing(Envelope[] envelopes)
        {
            using (var conn = new NpgsqlConnection(Settings.ConnectionString))
            {
                await conn.OpenAsync();

                await conn.CreateCommand(_deleteOutgoingEnvelopesSql)
                    .With("ids", envelopes.Select(x => x.Id).ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Uuid)
                    .ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteOutgoing(Envelope envelope)
        {
            using (var conn = new NpgsqlConnection(Settings.ConnectionString))
            {
                await conn.OpenAsync();

                await conn.CreateCommand(_deleteOutgoingEnvelopeSql)
                    .With("id", envelope.Id, NpgsqlDbType.Uuid)
                    .ExecuteNonQueryAsync();
            }
        }

        public async Task MoveToDeadLetterStorage(ErrorReport[] errors)
        {
            using (var conn = new NpgsqlConnection(Settings.ConnectionString))
            {
                await conn.OpenAsync();

                var tx = conn.BeginTransaction();

                var cmd = tx.CreateCommand(_deleteIncomingEnvelopesSql)
                    .With("ids", errors.Select(x => x.Id).ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Uuid);

                var builder = new CommandBuilder(cmd);


                foreach (var error in errors)
                {
                    var id = builder.AddParameter(error.Id, NpgsqlDbType.Uuid);
                    var source = builder.AddParameter(error.Source, NpgsqlDbType.Varchar);
                    var messageType = builder.AddParameter(error.MessageType, NpgsqlDbType.Varchar);
                    var explanation = builder.AddParameter(error.Explanation, NpgsqlDbType.Varchar);
                    var exText = builder.AddParameter(error.ExceptionText, NpgsqlDbType.Varchar);
                    var exType = builder.AddParameter(error.ExceptionType, NpgsqlDbType.Varchar);
                    var exMessage = builder.AddParameter(error.ExceptionMessage, NpgsqlDbType.Varchar);
                    var body = builder.AddParameter(error.RawData, NpgsqlDbType.Bytea);

                    builder.Append(
                        $"insert into {Settings.SchemaName}.{DeadLetterTable} (id, source, message_type, explanation, exception_text, exception_type, exception_message, body) values (:{id.ParameterName}, :{source.ParameterName}, :{messageType.ParameterName}, :{explanation.ParameterName}, :{exText.ParameterName}, :{exType.ParameterName}, :{exMessage.ParameterName}, :{body.ParameterName});");
                }

                builder.Apply();

                await cmd.ExecuteNonQueryAsync();
            }



        }

        public async Task ScheduleExecution(Envelope[] envelopes)
        {
            var cmd = new NpgsqlCommand();
            var builder = new CommandBuilder(cmd);

            foreach (var envelope in envelopes)
            {
                var id = builder.AddParameter(envelope.Id, NpgsqlDbType.Uuid);
                var time = builder.AddParameter(envelope.ExecutionTime, NpgsqlDbType.TimestampTz);
                var attempts = builder.AddParameter(envelope.Attempts, NpgsqlDbType.Integer);

                builder.Append(
                    $"update {Settings.SchemaName}.{IncomingTable} set execution_time = :{time.ParameterName}, status = \'{TransportConstants.Scheduled}\', attempts = :{attempts.ParameterName}, owner_id = {TransportConstants.AnyNode} where id = :{id.ParameterName};");
            }

            builder.Apply();

            using (var conn = new NpgsqlConnection(Settings.ConnectionString))
            {
                await conn.OpenAsync();

                cmd.Connection = conn;
                await cmd.ExecuteNonQueryAsync();
            }
        }


        public async Task IncrementIncomingEnvelopeAttempts(Envelope envelope)
        {
            using (var conn = new NpgsqlConnection(Settings.ConnectionString))
            {
                await conn.OpenAsync();

                await conn.CreateCommand(
                        $"update {Settings.SchemaName}.{IncomingTable} set attempts = :attempts where id = :id")
                    .With("attempts", envelope.Attempts, NpgsqlDbType.Integer)
                    .With("id", envelope.Id, NpgsqlDbType.Uuid)
                    .ExecuteNonQueryAsync();
            }
        }

        public async Task StoreIncoming(Envelope envelope)
        {
            envelope.EnsureData();

            using (var conn = new NpgsqlConnection(Settings.ConnectionString))
            {
                await conn.OpenAsync();

                var cmd = conn.CreateCommand($@"
insert into {Settings.SchemaName}.{IncomingTable}
  (id, status, owner_id, execution_time, attempts, body)
values
  (:id, :status, :owner, :time, :attempts, :body);
");

                await cmd
                    .With("id", envelope.Id, NpgsqlDbType.Uuid)
                    .With("status", envelope.Status, NpgsqlDbType.Varchar)
                    .With("owner", envelope.OwnerId, NpgsqlDbType.Integer)
                    .With("attempts", envelope.Attempts, NpgsqlDbType.Integer)
                    .With("time", envelope.ExecutionTime, NpgsqlDbType.TimestampTz)
                    .With("body", envelope.Serialize(), NpgsqlDbType.Bytea)
                    .ExecuteNonQueryAsync();
            }
        }

        public async Task StoreIncoming(Envelope[] envelopes)
        {
            var cmd = BuildIncomingStorageCommand(envelopes, Settings);

            using (var conn = new NpgsqlConnection(Settings.ConnectionString))
            {
                await conn.OpenAsync();

                cmd.Connection = conn;

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static NpgsqlCommand BuildIncomingStorageCommand(IEnumerable<Envelope> envelopes,
            PostgresqlSettings settings)
        {
            var cmd = new NpgsqlCommand();
            var builder = new CommandBuilder(cmd);

            foreach (var envelope in envelopes)
            {
                envelope.EnsureData();

                var id = builder.AddParameter(envelope.Id, NpgsqlDbType.Uuid);
                var status = builder.AddParameter(envelope.Status, NpgsqlDbType.Varchar);
                var owner = builder.AddParameter(envelope.OwnerId, NpgsqlDbType.Integer);
                var attempts = builder.AddParameter(envelope.Attempts, NpgsqlDbType.Integer);
                var time = builder.AddParameter(envelope.ExecutionTime, NpgsqlDbType.TimestampTz);
                var body = builder.AddParameter(envelope.Serialize(), NpgsqlDbType.Bytea);


                builder.Append(
                    $"insert into {settings.SchemaName}.{IncomingTable} (id, status, owner_id, execution_time, attempts, body) values (:{id.ParameterName}, :{status.ParameterName}, :{owner.ParameterName}, :{time.ParameterName}, :{attempts.ParameterName}, :{body.ParameterName});");
            }

            builder.Apply();

            return cmd;
        }

        public async Task DiscardAndReassignOutgoing(Envelope[] discards, Envelope[] reassigned, int nodeId)
        {
            var cmd = new NpgsqlCommand(_deleteOutgoingEnvelopesSql +
                                        $";update {Settings.SchemaName}.{OutgoingTable} set owner_id = :node where id = ANY(:rids)")
                .With("ids", discards.Select(x => x.Id).ToArray(), NpgsqlDbType.Array | NpgsqlDbType.Uuid)
                .With("node", nodeId, NpgsqlDbType.Integer)
                .With("rids", reassigned.Select(x => x.Id).ToArray(),NpgsqlDbType.Array | NpgsqlDbType.Uuid);

            using (var conn = new NpgsqlConnection(Settings.ConnectionString))
            {
                await conn.OpenAsync();

                cmd.Connection = conn;

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task StoreOutgoing(Envelope envelope, int ownerId)
        {
            envelope.EnsureData();

            using (var conn = new NpgsqlConnection(Settings.ConnectionString))
            {
                await conn.OpenAsync();

                await conn.CreateCommand(
                        $"insert into {Settings.SchemaName}.{OutgoingTable} (id, owner_id, destination, deliver_by, body) values (:id, :owner, :destination, :deliverBy, :body)")
                    .With("id", envelope.Id, NpgsqlDbType.Uuid)
                    .With("owner", ownerId, NpgsqlDbType.Integer)
                    .With("destination", envelope.Destination.ToString(), NpgsqlDbType.Varchar)
                    .With("deliverBy", envelope.DeliverBy, NpgsqlDbType.TimestampTz)
                    .With("body", envelope.Serialize(), NpgsqlDbType.Bytea)
                    .ExecuteNonQueryAsync();
            }
        }

        public async Task StoreOutgoing(Envelope[] envelopes, int ownerId)
        {
            var cmd = BuildOutgoingStorageCommand(envelopes, ownerId, Settings);

            using (var conn = new NpgsqlConnection(Settings.ConnectionString))
            {
                await conn.OpenAsync();

                cmd.Connection = conn;

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static NpgsqlCommand BuildOutgoingStorageCommand(Envelope[] envelopes, int ownerId,
            PostgresqlSettings settings)
        {
            var cmd = new NpgsqlCommand();
            var builder = new CommandBuilder(cmd);

            builder.AddNamedParameter("owner", ownerId).NpgsqlDbType = NpgsqlDbType.Integer;

            foreach (var envelope in envelopes)
            {
                envelope.EnsureData();

                var id = builder.AddParameter(envelope.Id, NpgsqlDbType.Uuid);
                var destination = builder.AddParameter(envelope.Destination.ToString(), NpgsqlDbType.Varchar);
                var deliverBy = builder.AddParameter(envelope.DeliverBy, NpgsqlDbType.TimestampTz);
                var body = builder.AddParameter(envelope.Serialize(), NpgsqlDbType.Bytea);

                builder.Append(
                    $"insert into {settings.SchemaName}.{OutgoingTable} (id, owner_id, destination, deliver_by, body) values (:{id.ParameterName}, :owner, :{destination.ParameterName}, :{deliverBy.ParameterName}, :{body.ParameterName});");
            }

            builder.Apply();

            return cmd;
        }



        public void Describe(TextWriter writer)
        {
            writer.WriteLine($"Persistent Envelope storage using Marten & Postgresql in schema '{Settings.SchemaName}'");
        }

        public Task ScheduleJob(Envelope envelope)
        {
            // TODO -- will be different later
            return StoreIncoming(envelope);
        }
    }
}
