using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Marten.Internal;
using Marten.Internal.Operations;
using Marten.Schema;
using Marten.Services;
using Marten.Util;
using NpgsqlTypes;
using Weasel.Core;
using Weasel.Postgresql;

namespace Jasper.Persistence.Marten.Persistence.Operations
{
    public class ScheduleEnvelope : IStorageOperation
    {
        private readonly Envelope _envelope;
        private readonly DbObjectName _table;

        public ScheduleEnvelope(DbObjectName table, Envelope envelope)
        {
            _table = table;
            _envelope = envelope;
        }

        public void ConfigureCommand(CommandBuilder builder, IMartenSession session)
        {
            var time = builder.AddParameter(_envelope.ExecutionTime, NpgsqlDbType.TimestampTz);
            var id = builder.AddParameter(_envelope.Id, NpgsqlDbType.Uuid);
            var attempts = builder.AddParameter(_envelope.Attempts, NpgsqlDbType.Integer);

            builder.Append(
                $"update {_table} set execution_time = :{time.ParameterName}, status = \'{EnvelopeStatus.Scheduled}\', attempts = :{attempts.ParameterName} where id = :{id.ParameterName}");
        }

        public void Postprocess(DbDataReader reader, IList<Exception> exceptions)
        {
            // Nothing
        }

        public Task PostprocessAsync(DbDataReader reader, IList<Exception> exceptions, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public OperationRole Role()
        {
            return OperationRole.Other;
        }

        public Type DocumentType => typeof(Envelope);
    }
}
