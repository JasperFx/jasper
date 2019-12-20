using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Configuration;
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

        protected DatabaseBackedEnvelopePersistence(DatabaseSettings databaseSettings, AdvancedSettings settings,
            IEnvelopeStorageAdmin admin, IDurabilityAgentStorage agentStorage)
        {
            this.DatabaseSettings = databaseSettings;
            Admin = admin;
            AgentStorage = agentStorage;

            Settings = settings;
            _cancellation = settings.Cancellation;

            _incrementIncomingAttempts = $"update {this.DatabaseSettings.SchemaName}.{IncomingTable} set attempts = @attempts where id = @id";
            _storeIncoming = $@"
insert into {this.DatabaseSettings.SchemaName}.{IncomingTable}
  (id, status, owner_id, execution_time, attempts, body)
values
  (@id, @status, @owner, @time, @attempts, @body);
";
            _insertOutgoingSql = $"insert into {this.DatabaseSettings.SchemaName}.{OutgoingTable} (id, owner_id, destination, deliver_by, body) values (@id, @owner, @destination, @deliverBy, @body)";
        }

        public AdvancedSettings Settings { get; }

        public IEnvelopeStorageAdmin Admin { get; }
        public IDurabilityAgentStorage AgentStorage { get; }

        public DatabaseSettings DatabaseSettings { get; }

        public Task DeleteOutgoing(Envelope envelope)
        {
            return DatabaseSettings.CreateCommand($"delete from {DatabaseSettings.SchemaName}.{OutgoingTable} where id = @id")
                .With("id", envelope.Id)
                .ExecuteOnce(_cancellation);

        }

        public Task DeleteIncomingEnvelope(Envelope envelope)
        {
            return DatabaseSettings.CreateCommand($"delete from {DatabaseSettings.SchemaName}.{IncomingTable} where id = @id")
                .With("id", envelope.Id)
                .ExecuteOnce(_cancellation);
        }



        public Task ScheduleExecution(Envelope[] envelopes)
        {
            var builder = DatabaseSettings.ToCommandBuilder();

            foreach (var envelope in envelopes)
            {
                var id = builder.AddParameter(envelope.Id);
                var time = builder.AddParameter(envelope.ExecutionTime.Value);
                var attempts = builder.AddParameter(envelope.Attempts);

                builder.Append(
                    $"update {DatabaseSettings.SchemaName}.{IncomingTable} set execution_time = @{time.ParameterName}, status = \'{EnvelopeStatus.Scheduled}\', attempts = @{attempts.ParameterName}, owner_id = {TransportConstants.AnyNode} where id = @{id.ParameterName};");
            }

            return builder.ApplyAndExecuteOnce(_cancellation);
        }


        public Task IncrementIncomingEnvelopeAttempts(Envelope envelope)
        {
            return DatabaseSettings.CreateCommand(_incrementIncomingAttempts)
                .With("attempts", envelope.Attempts)
                .With("id", envelope.Id)
                .ExecuteOnce(_cancellation);
        }

        public Task StoreIncoming(Envelope envelope)
        {
            return DatabaseSettings.CreateCommand(_storeIncoming)
                .With("id", envelope.Id)
                .With("status", envelope.Status.ToString())
                .With("owner", envelope.OwnerId)
                .With("attempts", envelope.Attempts)
                .With("time", envelope.ExecutionTime)
                .With("body", envelope.Serialize())
                .ExecuteOnce(_cancellation);
        }

        public Task StoreIncoming(DbTransaction tx, Envelope[] envelopes)
        {
            var cmd = BuildIncomingStorageCommand(envelopes, DatabaseSettings);

            cmd.Transaction = tx;
            cmd.Connection = tx.Connection;

            return cmd.ExecuteNonQueryAsync(_cancellation);
        }

        public async Task StoreIncoming(Envelope[] envelopes)
        {
            var cmd = BuildIncomingStorageCommand(envelopes, DatabaseSettings);

            using (var conn = DatabaseSettings.CreateConnection())
            {
                await conn.OpenAsync(_cancellation);

                cmd.Connection = conn;

                await cmd.ExecuteNonQueryAsync(_cancellation);
            }
        }

        public Task StoreOutgoing(Envelope envelope, int ownerId)
        {
            return DatabaseSettings.CreateCommand(_insertOutgoingSql)
                .With("id", envelope.Id)
                .With("owner", ownerId)
                .With("destination", envelope.Destination.ToString())
                .With("deliverBy", envelope.DeliverBy)
                .With("body", envelope.Serialize())
                .ExecuteOnce(_cancellation);
        }

        public Task StoreOutgoing(DbTransaction tx, Envelope[] envelopes)
        {
            var cmd = BuildOutgoingStorageCommand(envelopes, Settings.UniqueNodeId, DatabaseSettings);
            cmd.Connection = tx.Connection;
            cmd.Transaction = tx;

            return cmd.ExecuteNonQueryAsync(_cancellation);
        }

        public async Task StoreOutgoing(Envelope[] envelopes, int ownerId)
        {
            var cmd = BuildOutgoingStorageCommand(envelopes, ownerId, DatabaseSettings);

            using (var conn = DatabaseSettings.CreateConnection())
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
                var id = builder.AddParameter(envelope.Id);
                var status = builder.AddParameter(envelope.Status.ToString());
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
            DatabaseSettings
                .ExecuteSql(
                    $"delete from {DatabaseSettings.SchemaName}.{IncomingTable};delete from {DatabaseSettings.SchemaName}.{OutgoingTable};delete from {DatabaseSettings.SchemaName}.{DeadLetterTable}");

        }

        public Task ScheduleJob(Envelope envelope)
        {
            envelope.Status = EnvelopeStatus.Scheduled;
            envelope.OwnerId = TransportConstants.AnyNode;

            return StoreIncoming(envelope);
        }

        public Envelope[] AllIncomingEnvelopes()
        {
            using (var conn = DatabaseSettings.CreateConnection())
            {
                conn.Open();

                return conn
                    .CreateCommand(
                        $"select body, status, owner_id, execution_time, attempts from {DatabaseSettings.SchemaName}.{IncomingTable}")
                    .LoadEnvelopes();
            }
        }

        public Envelope[] AllOutgoingEnvelopes()
        {
            using (var conn = DatabaseSettings.CreateConnection())
            {
                conn.Open();

                return conn
                    .CreateCommand(
                        $"select body, '{EnvelopeStatus.Outgoing}', owner_id, NULL from {DatabaseSettings.SchemaName}.{OutgoingTable}")
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
