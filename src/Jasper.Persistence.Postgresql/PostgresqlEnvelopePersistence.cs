using System.IO;
using System.Threading.Tasks;
using Jasper.Persistence.Database;
using Jasper.Persistence.Durability;
using Jasper.Persistence.Postgresql.Schema;
using Jasper.Persistence.Postgresql.Util;
using Jasper.Transports;
using Npgsql;
using Weasel.Postgresql;
using Weasel.Core;


namespace Jasper.Persistence.Postgresql
{
    public class PostgresqlEnvelopePersistence : DatabaseBackedEnvelopePersistence
    {
        private readonly string _deleteIncomingEnvelopesSql;
        private readonly string _deleteOutgoingSql;
        private readonly string _reassignSql;
        private readonly string _deleteOutgoingEnvelopesSql;

        public PostgresqlEnvelopePersistence(PostgresqlSettings databaseSettings, AdvancedSettings settings) : base(databaseSettings,
            settings, new PostgresqlEnvelopeStorageAdmin(databaseSettings))
        {
            _deleteIncomingEnvelopesSql = $"delete from {databaseSettings.SchemaName}.{DatabaseConstants.IncomingTable} WHERE id = ANY(@ids);";
            _deleteOutgoingSql =
                $"delete from {databaseSettings.SchemaName}.{DatabaseConstants.OutgoingTable} where id = ANY(@ids)";

            _reassignSql = $"update {databaseSettings.SchemaName}.{DatabaseConstants.OutgoingTable} set owner_id = @owner where id = ANY(@ids)";
            _deleteOutgoingEnvelopesSql = $"delete from {databaseSettings.SchemaName}.{DatabaseConstants.OutgoingTable} WHERE id = ANY(@ids);";
        }


        public override Task MoveToDeadLetterStorage(ErrorReport[] errors)
        {
            var cmd = DatabaseSettings.CreateCommand(_deleteIncomingEnvelopesSql)
                .With("ids", errors);

            var builder = new CommandBuilder((NpgsqlCommand) cmd);

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
                    $"insert into {DatabaseSettings.SchemaName}.{DatabaseConstants.DeadLetterTable} (id, source, message_type, explanation, exception_text, exception_type, exception_message, body) values (@{id.ParameterName}, @{source.ParameterName}, @{messageType.ParameterName}, @{explanation.ParameterName}, @{exText.ParameterName}, @{exType.ParameterName}, @{exMessage.ParameterName}, @{body.ParameterName});");
            }

            return builder.Compile().ExecuteOnce(_cancellation);
        }

        public override Task DeleteIncomingEnvelopes(Envelope[] envelopes)
        {
            return DatabaseSettings.CreateCommand(_deleteIncomingEnvelopesSql)
                .With("ids", envelopes)
                .ExecuteOnce(_cancellation);
        }



        public override void Describe(TextWriter writer)
        {
            writer.WriteLine($"Persistent Envelope storage using Postgresql in schema '{DatabaseSettings.SchemaName}'");
        }

        protected override IDurableIncoming buildDurableIncoming(DurableStorageSession durableStorageSession,
            DatabaseSettings databaseSettings,
            AdvancedSettings settings)
        {
            return new PostgresqlDurableIncoming(durableStorageSession, databaseSettings, settings);
        }

        public override Task DiscardAndReassignOutgoing(Envelope[] discards, Envelope[] reassigned, int nodeId)
        {
            return DatabaseSettings.CreateCommand(_deleteOutgoingEnvelopesSql +
                                                  $";update {DatabaseSettings.SchemaName}.{DatabaseConstants.OutgoingTable} set owner_id = @node where id = ANY(@rids)")
                .With("ids", discards)
                .With("node", nodeId)
                .With("rids", reassigned)
                .ExecuteOnce(_cancellation);
        }

        public override Task DeleteOutgoing(Envelope[] envelopes)
        {
            return DatabaseSettings.CreateCommand(_deleteOutgoingEnvelopesSql)
                .With("ids", envelopes)
                .ExecuteOnce(_cancellation);
        }


        protected override string determineOutgoingEnvelopeSql(DatabaseSettings databaseSettings, AdvancedSettings settings)
        {
            return $"select body from {databaseSettings.SchemaName}.{DatabaseConstants.OutgoingTable} where owner_id = {TransportConstants.AnyNode} and destination = @destination LIMIT {settings.RecoveryBatchSize}";
        }

        public override Task Reassign(int ownerId, Envelope[] outgoing)
        {
            return _session.CreateCommand(_reassignSql)
                .With("owner", ownerId)
                .With("ids", outgoing)
                .ExecuteNonQueryAsync(_cancellation);
        }


        public override Task Delete(Envelope[] outgoing)
        {
            return _session.CreateCommand(_deleteOutgoingSql)
                .With("ids", outgoing)
                .ExecuteNonQueryAsync(_cancellation);
        }
    }
}
