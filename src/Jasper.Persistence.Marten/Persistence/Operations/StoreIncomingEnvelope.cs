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
using Weasel.Postgresql;

namespace Jasper.Persistence.Marten.Persistence.Operations
{
    public class StoreIncomingEnvelope : IStorageOperation
    {
        private readonly string _incomingTable;

        public StoreIncomingEnvelope(string incomingTable, Envelope envelope)
        {
            Envelope = envelope;
            _incomingTable = incomingTable;
        }

        public Envelope Envelope { get; }

        public void ConfigureCommand(CommandBuilder builder, IMartenSession session)
        {
            var bytes = Envelope.Serialize();

            var id = builder.AddParameter(Envelope.Id, NpgsqlDbType.Uuid);
            var owner = builder.AddParameter(Envelope.OwnerId, NpgsqlDbType.Integer);
            var status = builder.AddParameter(Envelope.Status.ToString(), NpgsqlDbType.Varchar);
            var executionTime =
                builder.AddParameter(
                    Envelope.ExecutionTime,
                    NpgsqlDbType.TimestampTz);

            var body = builder.AddParameter(bytes, NpgsqlDbType.Bytea);

            var sql = $@"
insert into {_incomingTable}
  (id, owner_id, status, execution_time, body)
values
  (:{id.ParameterName}, :{owner.ParameterName}, :{status.ParameterName}, :{executionTime.ParameterName}, :{body.ParameterName})";
            builder.Append(sql);
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
