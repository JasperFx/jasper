using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;

namespace Jasper.Persistence.Database
{
    public abstract class DatabaseBackedEnvelopePersistence : DataAccessor, IEnvelopePersistence
    {
        protected readonly CancellationToken _cancellation;
        private readonly string _incrementIncomingAttempts;
        private readonly string _storeIncoming;
        private readonly string _insertOutgoingSql;

        protected DatabaseBackedEnvelopePersistence(DatabaseSettings settings, JasperOptions options, IEnvelopeStorageAdmin admin, IDurabilityAgentStorage agentStorage)
        {
            this.Settings = settings;
            Admin = admin;
            AgentStorage = agentStorage;

            Options = options;
            _cancellation = options.Cancellation;

            _incrementIncomingAttempts = $"update {this.Settings.SchemaName}.{IncomingTable} set attempts = @attempts where id = @id";
            _storeIncoming = $@"
insert into {this.Settings.SchemaName}.{IncomingTable}
  (id, status, owner_id, execution_time, attempts, body)
values
  (@id, @status, @owner, @time, @attempts, @body);
";
            _insertOutgoingSql = $"insert into {this.Settings.SchemaName}.{OutgoingTable} (id, owner_id, destination, deliver_by, body) values (@id, @owner, @destination, @deliverBy, @body)";
        }

        public JasperOptions Options { get; }

        public IEnvelopeStorageAdmin Admin { get; }
        public IDurabilityAgentStorage AgentStorage { get; }

        public DatabaseSettings Settings { get; }

        public Task DeleteOutgoing(Envelope envelope)
        {
            return Settings.CreateCommand($"delete from {Settings.SchemaName}.{OutgoingTable} where id = @id")
                .With("id", envelope.Id)
                .ExecuteOnce(_cancellation);

        }

        public Task DeleteIncomingEnvelope(Envelope envelope)
        {
            return Settings.CreateCommand($"delete from {Settings.SchemaName}.{IncomingTable} where id = @id")
                .With("id", envelope.Id)
                .ExecuteOnce(_cancellation);
        }



        public Task ScheduleExecution(Envelope[] envelopes)
        {
            var builder = Settings.ToCommandBuilder();

            foreach (var envelope in envelopes)
            {
                var id = builder.AddParameter(envelope.Id);
                var time = builder.AddParameter(envelope.ExecutionTime.Value);
                var attempts = builder.AddParameter(envelope.Attempts);

                builder.Append(
                    $"update {Settings.SchemaName}.{IncomingTable} set execution_time = @{time.ParameterName}, status = \'{TransportConstants.Scheduled}\', attempts = @{attempts.ParameterName}, owner_id = {TransportConstants.AnyNode} where id = @{id.ParameterName};");
            }

            return builder.ApplyAndExecuteOnce(_cancellation);
        }


        public Task IncrementIncomingEnvelopeAttempts(Envelope envelope)
        {
            return Settings.CreateCommand(_incrementIncomingAttempts)
                .With("attempts", envelope.Attempts)
                .With("id", envelope.Id)
                .ExecuteOnce(_cancellation);
        }

        public Task StoreIncoming(Envelope envelope)
        {
            envelope.EnsureData();

            return Settings.CreateCommand(_storeIncoming)
                .With("id", envelope.Id)
                .With("status", envelope.Status)
                .With("owner", envelope.OwnerId)
                .With("attempts", envelope.Attempts)
                .With("time", envelope.ExecutionTime)
                .With("body", envelope.Serialize())
                .ExecuteOnce(_cancellation);
        }

        public Task StoreIncoming(DbTransaction tx, Envelope[] envelopes)
        {
            var cmd = BuildIncomingStorageCommand(envelopes, Settings);

            cmd.Transaction = tx;
            cmd.Connection = tx.Connection;

            return cmd.ExecuteNonQueryAsync(_cancellation);
        }

        public async Task StoreIncoming(Envelope[] envelopes)
        {
            var cmd = BuildIncomingStorageCommand(envelopes, Settings);

            using (var conn = Settings.CreateConnection())
            {
                await conn.OpenAsync(_cancellation);

                cmd.Connection = conn;

                await cmd.ExecuteNonQueryAsync(_cancellation);
            }
        }

        public Task StoreOutgoing(Envelope envelope, int ownerId)
        {
            envelope.EnsureData();

            return Settings.CreateCommand(_insertOutgoingSql)
                .With("id", envelope.Id)
                .With("owner", ownerId)
                .With("destination", envelope.Destination.ToString())
                .With("deliverBy", envelope.DeliverBy)
                .With("body", envelope.Serialize())
                .ExecuteOnce(_cancellation);
        }

        public Task StoreOutgoing(DbTransaction tx, Envelope[] envelopes)
        {
            var cmd = BuildOutgoingStorageCommand(envelopes, Options.UniqueNodeId, Settings);
            cmd.Connection = tx.Connection;
            cmd.Transaction = tx;

            return cmd.ExecuteNonQueryAsync(_cancellation);
        }

        public async Task StoreOutgoing(Envelope[] envelopes, int ownerId)
        {
            var cmd = BuildOutgoingStorageCommand(envelopes, ownerId, Settings);

            using (var conn = Settings.CreateConnection())
            {
                await conn.OpenAsync(_cancellation);

                cmd.Connection = conn;

                await cmd.ExecuteNonQueryAsync(_cancellation);
            }
        }

                internal static DbCommand BuildIncomingStorageCommand(IEnumerable<Envelope> envelopes, DatabaseSettings settings)
        {
            var cmd = settings.CreateEmptyCommand();
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

        public static DbCommand BuildOutgoingStorageCommand(Envelope[] envelopes, int ownerId,
            DatabaseSettings settings)
        {
            var cmd = settings.CreateEmptyCommand();
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
            Settings
                .ExecuteSql(
                    $"delete from {Settings.SchemaName}.{IncomingTable};delete from {Settings.SchemaName}.{OutgoingTable};delete from {Settings.SchemaName}.{DeadLetterTable}");

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
            using (var conn = Settings.CreateConnection())
            {
                conn.Open();

                return conn
                    .CreateCommand(
                        $"select body, status, owner_id, execution_time, attempts from {Settings.SchemaName}.{IncomingTable}")
                    .LoadEnvelopes();
            }
        }

        public Envelope[] AllOutgoingEnvelopes()
        {
            using (var conn = Settings.CreateConnection())
            {
                conn.Open();

                return conn
                    .CreateCommand(
                        $"select body, '{TransportConstants.Outgoing}', owner_id, NULL from {Settings.SchemaName}.{OutgoingTable}")
                    .LoadEnvelopes();
            }
        }


        public abstract Task MoveToDeadLetterStorage(ErrorReport[] errors);
        public abstract Task DeleteIncomingEnvelopes(Envelope[] envelopes);
        public abstract Task DiscardAndReassignOutgoing(Envelope[] discards, Envelope[] reassigned, int nodeId);
        public abstract Task DeleteOutgoing(Envelope[] envelopes);
        public abstract void Describe(TextWriter writer);
    }
}
