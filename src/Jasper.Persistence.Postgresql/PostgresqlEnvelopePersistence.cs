using System.IO;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Persistence.Database;
using Jasper.Persistence.Postgresql.Schema;
using Jasper.Persistence.Postgresql.Util;

namespace Jasper.Persistence.Postgresql
{
    public class PostgresqlEnvelopePersistence : DatabaseBackedEnvelopePersistence
    {
        private readonly string _deleteIncomingEnvelopesSql;
        private readonly string _deleteOutgoingEnvelopesSql;

        public PostgresqlEnvelopePersistence(PostgresqlSettings databaseSettings, AdvancedSettings settings) : base(databaseSettings,
            settings, new PostgresqlEnvelopeStorageAdmin(databaseSettings),
            new PostgresqlDurabilityAgentStorage(databaseSettings, settings))
        {
            _deleteIncomingEnvelopesSql = $"delete from {databaseSettings.SchemaName}.{IncomingTable} WHERE id = ANY(@ids);";
            _deleteOutgoingEnvelopesSql = $"delete from {databaseSettings.SchemaName}.{OutgoingTable} WHERE id = ANY(@ids);";
        }


        public override Task MoveToDeadLetterStorage(ErrorReport[] errors)
        {
            var cmd = DatabaseSettings.CreateCommand(_deleteIncomingEnvelopesSql)
                .With("ids", errors);

            var builder = new CommandBuilder(cmd);


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
                    $"insert into {DatabaseSettings.SchemaName}.{DeadLetterTable} (id, source, message_type, explanation, exception_text, exception_type, exception_message, body) values (@{id.ParameterName}, @{source.ParameterName}, @{messageType.ParameterName}, @{explanation.ParameterName}, @{exText.ParameterName}, @{exType.ParameterName}, @{exMessage.ParameterName}, @{body.ParameterName});");
            }

            return builder.ApplyAndExecuteOnce(_cancellation);
        }

        public override Task DeleteIncomingEnvelopes(Envelope[] envelopes)
        {
            return DatabaseSettings.CreateCommand(_deleteIncomingEnvelopesSql)
                .With("ids", envelopes)
                .ExecuteOnce(_cancellation);
        }

        public override Task DiscardAndReassignOutgoing(Envelope[] discards, Envelope[] reassigned, int nodeId)
        {
            return DatabaseSettings.CreateCommand(_deleteOutgoingEnvelopesSql +
                                          $";update {DatabaseSettings.SchemaName}.{OutgoingTable} set owner_id = @node where id = ANY(@rids)")
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

        public override void Describe(TextWriter writer)
        {
            writer.WriteLine($"Persistent Envelope storage using Postgresql in schema '{DatabaseSettings.SchemaName}'");
        }
    }
}
