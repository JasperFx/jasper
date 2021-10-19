using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Weasel.Core;

namespace Jasper.Persistence.Database
{
    public partial class DatabaseBackedEnvelopePersistence
    {
        public abstract Task MoveToDeadLetterStorage(ErrorReport[] errors);
        public abstract Task DeleteIncomingEnvelopes(Envelope[] envelopes);

        public abstract Task<IReadOnlyList<Envelope>> LoadPageOfLocallyOwnedIncoming();
        public abstract Task ReassignIncoming(int ownerId, IReadOnlyList<Envelope> incoming);

        public static async Task<Envelope> ReadIncoming(DbDataReader reader, CancellationToken cancellation = default)
        {
            // body, id, status, owner_id, execution_time, attempts

            var bytes = await reader.GetFieldValueAsync<byte[]>(0, cancellation);
            var envelope = Envelope.Deserialize(bytes);

            envelope.Id = await reader.GetFieldValueAsync<Guid>(1, cancellation);
            envelope.Status = Enum.Parse<EnvelopeStatus>(await reader.GetFieldValueAsync<string>(2, cancellation));
            envelope.OwnerId = await reader.GetFieldValueAsync<int>(3, cancellation);
            if (!(await reader.IsDBNullAsync(4, cancellation)))
            {
                envelope.ExecutionTime = await reader.GetFieldValueAsync<DateTimeOffset>(4, cancellation);
            }

            envelope.Attempts = await reader.GetFieldValueAsync<int>(5, cancellation);

            return envelope;
        }

        public Task DeleteIncomingEnvelope(Envelope envelope)
        {
            return DatabaseSettings
                .CreateCommand($"delete from {DatabaseSettings.SchemaName}.{DatabaseConstants.IncomingTable} where id = @id")
                .With("id", envelope.Id)
                .ExecuteOnce(_cancellation);
        }

        public Task StoreIncoming(DbTransaction tx, Envelope[] envelopes)
        {
            var cmd = BuildIncomingStorageCommand(envelopes, DatabaseSettings);

            cmd.Transaction = tx;
            cmd.Connection = tx.Connection;

            return cmd.ExecuteNonQueryAsync(_cancellation);
        }

        internal static DbCommand BuildIncomingStorageCommand(IEnumerable<Envelope> envelopes,
            DatabaseSettings settings)
        {
            var builder = settings.ToCommandBuilder();

            foreach (var envelope in envelopes)
            {
                var id = builder.AddParameter(envelope.Id);
                var status = builder.AddParameter(envelope.Status.ToString());
                var owner = builder.AddParameter(envelope.OwnerId);
                var attempts = builder.AddParameter(envelope.Attempts);
                var time = builder.AddParameter(envelope.ExecutionTime);
                var body = builder.AddParameter(envelope.Serialize());


                builder.Append(
                    $"insert into {settings.SchemaName}.{DatabaseConstants.IncomingTable} (id, status, owner_id, execution_time, attempts, body) values (@{id.ParameterName}, @{status.ParameterName}, @{owner.ParameterName}, @{time.ParameterName}, @{attempts.ParameterName}, @{body.ParameterName});");
            }


            return builder.Compile();
        }

        public Task MoveToDeadLetterStorage(Envelope envelope, Exception ex)
        {
            return MoveToDeadLetterStorage(new[] {new ErrorReport(envelope, ex)});
        }

        public Task IncrementIncomingEnvelopeAttempts(Envelope envelope)
        {
            return DatabaseSettings.CreateCommand($"update {DatabaseSettings.SchemaName}.{DatabaseConstants.IncomingTable} set attempts = @attempts where id = @id")
                .With("attempts", envelope.Attempts)
                .With("id", envelope.Id)
                .ExecuteOnce(_cancellation);
        }

        public Task StoreIncoming(Envelope envelope)
        {
            return DatabaseSettings.CreateCommand($@"
insert into {DatabaseSettings.SchemaName}.{DatabaseConstants.IncomingTable}
  (id, status, owner_id, execution_time, attempts, body)
values
  (@id, @status, @owner, @time, @attempts, @body);
")
                .With("id", envelope.Id)
                .With("status", envelope.Status.ToString())
                .With("owner", envelope.OwnerId)
                .With("attempts", envelope.Attempts)
                .With("time", envelope.ExecutionTime)
                .With("body", envelope.Serialize())
                .ExecuteOnce(_cancellation);
        }

        public async Task StoreIncoming(Envelope[] envelopes)
        {
            var cmd = BuildIncomingStorageCommand(envelopes, DatabaseSettings);

            await using var conn = DatabaseSettings.CreateConnection();
            await conn.OpenAsync(_cancellation);

            cmd.Connection = conn;

            await cmd.ExecuteNonQueryAsync(_cancellation);
        }



    }
}
