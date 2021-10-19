using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Util;
using Weasel.Core;
using DbCommandBuilder = Weasel.Core.DbCommandBuilder;

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
            var envelope = new Envelope
            {
                Data = await reader.GetFieldValueAsync<byte[]>(0, cancellation),
                Id = await reader.GetFieldValueAsync<Guid>(1, cancellation),
                Status = Enum.Parse<EnvelopeStatus>(await reader.GetFieldValueAsync<string>(2, cancellation)),
                OwnerId = await reader.GetFieldValueAsync<int>(3, cancellation)
            };

            if (!(await reader.IsDBNullAsync(4, cancellation)))
            {
                envelope.ExecutionTime = await reader.GetFieldValueAsync<DateTimeOffset>(4, cancellation);
            }

            envelope.Attempts = await reader.GetFieldValueAsync<int>(5, cancellation);

            envelope.CausationId = await reader.GetFieldValueAsync<Guid>(6, cancellation);
            envelope.CorrelationId = await reader.GetFieldValueAsync<Guid>(7, cancellation);

            if (!await reader.IsDBNullAsync(8, cancellation))
            {
                envelope.SagaId = await reader.GetFieldValueAsync<string>(8, cancellation);
            }

            envelope.MessageType = await reader.GetFieldValueAsync<string>(9, cancellation);
            envelope.ContentType = await reader.GetFieldValueAsync<string>(10, cancellation);

            if (!await reader.IsDBNullAsync(11, cancellation))
            {
                envelope.ReplyRequested = await reader.GetFieldValueAsync<string>(11, cancellation);
            }

            envelope.AckRequested = await reader.GetFieldValueAsync<bool>(12, cancellation);

            if (!await reader.IsDBNullAsync(13, cancellation))
            {
                envelope.ReplyUri = (await reader.GetFieldValueAsync<string>(13, cancellation)).ToUri();
            }

            if (!await reader.IsDBNullAsync(14, cancellation))
            {
                envelope.ReceivedAt = (await reader.GetFieldValueAsync<string>(14, cancellation)).ToUri();
            }

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
                BuildIncomingStorageCommand(settings, builder, envelope);
            }

            return builder.Compile();
        }

        public static void BuildIncomingStorageCommand(DatabaseSettings settings, DbCommandBuilder builder, Envelope envelope)
        {
            var list = new List<DbParameter>();

            list.Add(builder.AddParameter(envelope.Data));
            list.Add(builder.AddParameter(envelope.Id));
            list.Add(builder.AddParameter(envelope.Status.ToString()));
            list.Add(builder.AddParameter(envelope.OwnerId));
            list.Add(builder.AddParameter(envelope.ExecutionTime));
            list.Add(builder.AddParameter(envelope.Attempts));

            list.Add(builder.AddParameter(envelope.CausationId));
            list.Add(builder.AddParameter(envelope.CorrelationId));
            list.Add(builder.AddParameter(envelope.SagaId));
            list.Add(builder.AddParameter(envelope.MessageType));
            list.Add(builder.AddParameter(envelope.ContentType));
            list.Add(builder.AddParameter(envelope.ReplyRequested));
            list.Add(builder.AddParameter(envelope.AckRequested));
            list.Add(builder.AddParameter(envelope.ReplyUri?.ToString()));
            list.Add(builder.AddParameter(envelope.ReceivedAt?.ToString()));

            var parameterList = list.Select(x => $"@{x.ParameterName}").Join(", ");

            builder.Append(
                $@"insert into {settings.SchemaName}.{DatabaseConstants.IncomingTable} ({DatabaseConstants.IncomingFields}) values ({parameterList});");
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
            var builder = DatabaseSettings.ToCommandBuilder();
            BuildIncomingStorageCommand(DatabaseSettings, builder, envelope);

            var cmd = builder.Compile();
            return cmd.ExecuteOnce(_cancellation);

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
