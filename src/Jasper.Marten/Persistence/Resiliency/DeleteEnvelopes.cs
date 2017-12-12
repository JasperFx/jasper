using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Bus.Runtime;
using Marten.Schema;
using Marten.Services;
using Marten.Util;
using NpgsqlTypes;

namespace Jasper.Marten.Persistence.Resiliency
{
    public class DeleteEnvelopes : IStorageOperation
    {
        private readonly DbObjectName _table;
        private readonly Guid[] _idlist;

        public DeleteEnvelopes(DbObjectName table, IEnumerable<Envelope> envelopes)
        {
            _table = table;
            _idlist = envelopes.Select(x => x.Id).ToArray();
        }

        public DeleteEnvelopes(DbObjectName table, Guid[] idlist)
        {
            _table = table;
            _idlist = idlist;
        }

        public void ConfigureCommand(CommandBuilder builder)
        {
            var idList = builder.AddParameter(_idlist, NpgsqlDbType.Array | NpgsqlDbType.Uuid);
            builder.Append("delete from ");
            builder.Append(_table);
            builder.Append(" where id = ANY(:");
            builder.Append(idList.ParameterName);
            builder.Append(")");
        }

        public Type DocumentType => typeof(Envelope);
    }

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
