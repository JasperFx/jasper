using System;
using Marten.Schema;
using Marten.Services;
using Marten.Util;
using NpgsqlTypes;

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

        public void ConfigureCommand(CommandBuilder builder)
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

        public Type DocumentType => typeof(Envelope);
    }
}
