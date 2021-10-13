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
    public class StoreOutgoingEnvelope : IStorageOperation
    {
        private readonly string _outgoingTable;
        private readonly int _ownerId;

        public StoreOutgoingEnvelope(string outgoingTable, Envelope envelope, int ownerId)
        {
            Envelope = envelope;
            _outgoingTable = outgoingTable;
            _ownerId = ownerId;
        }

        public Envelope Envelope { get; }

        public void ConfigureCommand(CommandBuilder builder, IMartenSession session)
        {
            var bytes = Envelope.Serialize();

            var id = builder.AddParameter(Envelope.Id, NpgsqlDbType.Uuid);
            var owner = builder.AddParameter(_ownerId, NpgsqlDbType.Integer);
            var destination = builder.AddParameter(Envelope.Destination.ToString(), NpgsqlDbType.Varchar);
            var deliverBy =
                builder.AddParameter(
                    Envelope.DeliverBy,
                    NpgsqlDbType.TimestampTz);

            var body = builder.AddParameter(bytes, NpgsqlDbType.Bytea);

            var sql = $@"
insert into {_outgoingTable}
  (id, owner_id, destination, deliver_by, body)
values
  (:{id.ParameterName}, :{owner.ParameterName}, :{destination.ParameterName}, :{deliverBy.ParameterName}, :{body.ParameterName})";
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
