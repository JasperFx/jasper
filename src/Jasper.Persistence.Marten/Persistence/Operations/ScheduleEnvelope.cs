using System;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Marten.Schema;
using Marten.Services;
using Marten.Util;
using NpgsqlTypes;

namespace Jasper.Marten.Persistence.Operations
{
    public class ScheduleEnvelope : IStorageOperation
    {
        private readonly DbObjectName _table;
        private readonly Envelope _envelope;

        public ScheduleEnvelope(DbObjectName table, Envelope envelope)
        {
            _table = table;
            _envelope = envelope;
        }

        public void ConfigureCommand(CommandBuilder builder)
        {
            var time = builder.AddParameter(_envelope.ExecutionTime, NpgsqlDbType.TimestampTZ);
            var id = builder.AddParameter(_envelope.Id, NpgsqlDbType.Uuid);
            var attempts = builder.AddParameter(_envelope.Attempts, NpgsqlDbType.Integer);

            builder.Append($"update {_table} set execution_time = :{time.ParameterName}, status = \'{TransportConstants.Scheduled}\', attempts = :{attempts.ParameterName} where id = :{id.ParameterName}");
        }

        public Type DocumentType => typeof(Envelope);
    }
}