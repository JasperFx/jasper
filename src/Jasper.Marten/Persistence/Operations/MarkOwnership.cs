using System;
using System.Collections.Generic;
using System.Linq;
using Jasper.Messaging.Runtime;
using Marten.Schema;
using Marten.Services;
using Marten.Util;
using NpgsqlTypes;

namespace Jasper.Marten.Persistence.Operations
{
    public class MarkOwnership : IStorageOperation
    {
        private readonly DbObjectName _table;
        private readonly int _ownerId;
        private readonly Guid[] _idlist;

        public MarkOwnership(DbObjectName table, int ownerId, IEnumerable<Envelope> envelopes)
        {
            _table = table;
            _ownerId = ownerId;
            _idlist = envelopes.Select(x => x.Id).ToArray();
        }

        public void ConfigureCommand(CommandBuilder builder)
        {
            var idList = builder.AddParameter(_idlist, NpgsqlDbType.Array | NpgsqlDbType.Uuid);
            var owner = builder.AddParameter(_ownerId, NpgsqlDbType.Integer);

            builder.Append("update ");
            builder.Append(_table);
            builder.Append(" set owner_id = :");
            builder.Append(owner.ParameterName);
            builder.Append(" where id = ANY(:");
            builder.Append(idList.ParameterName);
            builder.Append(")");
        }

        public Type DocumentType => typeof(Envelope);
    }
}
