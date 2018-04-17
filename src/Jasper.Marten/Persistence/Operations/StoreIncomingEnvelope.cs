using System;
using Jasper.Messaging.Runtime;
using Marten.Schema;
using Marten.Services;
using Marten.Util;
using NpgsqlTypes;

namespace Jasper.Marten.Persistence.Operations
{
    public class StoreIncomingEnvelope : IStorageOperation
    {
        public Envelope Envelope { get; }
        private readonly DbObjectName _incomingTable;

        public StoreIncomingEnvelope(DbObjectName incomingTable, Envelope envelope)
        {
            Envelope = envelope;
            _incomingTable = incomingTable;
        }

        public void ConfigureCommand(CommandBuilder builder)
        {
            Envelope.EnsureData();
            var bytes = Envelope.Serialize();

            var id = builder.AddParameter(Envelope.Id, NpgsqlDbType.Uuid);
            var owner = builder.AddParameter(Envelope.OwnerId, NpgsqlDbType.Integer);
            var status = builder.AddParameter(Envelope.Status, NpgsqlDbType.Varchar);
            var executionTime =
                builder.AddParameter(
                    Envelope.ExecutionTime,
                    NpgsqlDbType.TimestampTZ);

            var body = builder.AddParameter(bytes, NpgsqlDbType.Bytea);

            var sql = $@"
insert into {_incomingTable}
  (id, owner_id, status, execution_time, body)
values
  (:{id.ParameterName}, :{owner.ParameterName}, :{status.ParameterName}, :{executionTime.ParameterName}, :{body.ParameterName})";
            builder.Append(sql);
        }

        public Type DocumentType => typeof(Envelope);
    }
}
