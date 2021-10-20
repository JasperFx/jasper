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

        public static DbCommand BuildIncomingStorageCommand(IEnumerable<Envelope> envelopes,
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

        public static void ConfigureDeadLetterCommands(ErrorReport[] errors, DbCommandBuilder builder,
            DatabaseSettings databaseSettings)
        {
            foreach (var error in errors)
            {
                var list = new List<DbParameter>();

                list.Add(builder.AddParameter(error.Id));
                list.Add(builder.AddParameter(error.Envelope.ExecutionTime));
                list.Add(builder.AddParameter(error.Envelope.Attempts));
                list.Add(builder.AddParameter(error.Envelope.Data));
                list.Add(builder.AddParameter(error.Envelope.CausationId));
                list.Add(builder.AddParameter(error.Envelope.CorrelationId));
                list.Add(builder.AddParameter(error.Envelope.SagaId));
                list.Add(builder.AddParameter(error.Envelope.MessageType));
                list.Add(builder.AddParameter(error.Envelope.ContentType));
                list.Add(builder.AddParameter(error.Envelope.ReplyRequested));
                list.Add(builder.AddParameter(error.Envelope.AckRequested));
                list.Add(builder.AddParameter(error.Envelope.ReplyUri?.ToString()));
                list.Add(builder.AddParameter(error.Envelope.ReceivedAt?.ToString()));
                list.Add(builder.AddParameter(error.Envelope.Source));
                list.Add(builder.AddParameter(error.Explanation));
                list.Add(builder.AddParameter(error.ExceptionText));
                list.Add(builder.AddParameter(error.ExceptionType));
                list.Add(builder.AddParameter(error.ExceptionMessage));

                var parameterList = list.Select(x => $"@{x.ParameterName}").Join(", ");

                builder.Append(
                    $"insert into {databaseSettings.SchemaName}.{DatabaseConstants.DeadLetterTable} ({DatabaseConstants.DeadLetterFields}) values ({parameterList});");
            }
        }

        public async Task<ErrorReport> LoadDeadLetterEnvelope(Guid id)
        {
            await using var conn = DatabaseSettings.CreateConnection();
            await conn.OpenAsync(_cancellation);

            var cmd = conn.CreateCommand(
                $"select {DatabaseConstants.DeadLetterFields} from {DatabaseSettings.SchemaName}.{DatabaseConstants.DeadLetterTable} where id = @id");
            cmd.With("id", id);

            await using var reader = await cmd.ExecuteReaderAsync(_cancellation);
            if (!await reader.ReadAsync(_cancellation)) return null;

            var envelope = new Envelope
            {
                Id = await reader.GetFieldValueAsync<Guid>(0, _cancellation)
            };

            if (!await reader.IsDBNullAsync(1, _cancellation))
            {
                envelope.ExecutionTime = await reader.GetFieldValueAsync<DateTimeOffset>(1, _cancellation);
            }

            envelope.Attempts = await reader.GetFieldValueAsync<int>(2, _cancellation);
            envelope.Data = await reader.GetFieldValueAsync<byte[]>(3, _cancellation);
            envelope.CausationId = await reader.GetFieldValueAsync<Guid>(4, _cancellation);
            envelope.CorrelationId = await reader.GetFieldValueAsync<Guid>(5, _cancellation);
            if (!await reader.IsDBNullAsync(6, _cancellation))
            {
                envelope.SagaId = await reader.GetFieldValueAsync<string>(6, _cancellation);
            }
            envelope.MessageType = await reader.GetFieldValueAsync<string>(7, _cancellation);
            envelope.ContentType = await reader.GetFieldValueAsync<string>(8, _cancellation);
            if (!await reader.IsDBNullAsync(9, _cancellation))
            {
                envelope.ReplyRequested = await reader.GetFieldValueAsync<string>(9, _cancellation);
            }

            envelope.AckRequested = await reader.GetFieldValueAsync<bool>(10, _cancellation);
            if (!await reader.IsDBNullAsync(11, _cancellation))
            {
                envelope.ReplyUri = (await reader.GetFieldValueAsync<string>(11, _cancellation)).ToUri();
            }

            if (!await reader.IsDBNullAsync(12, _cancellation))
            {
                envelope.ReceivedAt = (await reader.GetFieldValueAsync<string>(12, _cancellation)).ToUri();
            }

            if (!await reader.IsDBNullAsync(13, _cancellation))
            {
                envelope.Source = await reader.GetFieldValueAsync<string>(13, _cancellation);
            }

            var report = new ErrorReport(envelope)
            {
                Explanation = await reader.GetFieldValueAsync<string>(14, _cancellation),
                ExceptionText = await reader.GetFieldValueAsync<string>(15, _cancellation),
                ExceptionType = await reader.GetFieldValueAsync<string>(16, _cancellation),
                ExceptionMessage = await reader.GetFieldValueAsync<string>(17, _cancellation),
            };

            return report;
        }
    }
}
