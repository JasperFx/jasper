using System;
using Jasper.Messaging.Runtime;
using Marten.Schema;
using Marten.Services;
using Marten.Util;
using NpgsqlTypes;

namespace Jasper.Marten.Persistence.Operations
{
    public class DeleteEnvelope : IStorageOperation
    {
        public Envelope Envelope { get; }
        private readonly DbObjectName _table;

        public DeleteEnvelope(DbObjectName table, Envelope envelope)
        {
            Envelope = envelope;
            _table = table;
        }

        public void ConfigureCommand(CommandBuilder builder)
        {
            var idList = builder.AddParameter(Envelope.Id, NpgsqlDbType.Uuid);
            builder.Append("delete from ");
            builder.Append(_table);
            builder.Append(" where id = :");
            builder.Append(idList.ParameterName);
            builder.Append("");
        }

        public Type DocumentType => typeof(Envelope);
    }
}