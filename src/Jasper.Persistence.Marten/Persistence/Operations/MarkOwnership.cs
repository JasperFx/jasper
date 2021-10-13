using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Marten.Internal;
using Marten.Internal.Operations;
using NpgsqlTypes;
using Weasel.Core;
using Weasel.Postgresql;

namespace Jasper.Persistence.Marten.Persistence.Operations
{
    public class MarkOwnership : IStorageOperation
    {
        private readonly Guid[] _idlist;
        private readonly int _ownerId;
        private readonly DbObjectName _table;

        public MarkOwnership(DbObjectName table, int ownerId, IEnumerable<Envelope> envelopes)
        {
            _table = table;
            _ownerId = ownerId;
            _idlist = envelopes.Select(x => x.Id).ToArray();
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

        public void ConfigureCommand(CommandBuilder builder, IMartenSession session)
        {
            var idList = builder.AddParameter(_idlist, NpgsqlDbType.Array | NpgsqlDbType.Uuid);
            var owner = builder.AddParameter(_ownerId, NpgsqlDbType.Integer);

            builder.Append("update ");
            builder.Append(_table.QualifiedName);
            builder.Append(" set owner_id = :");
            builder.Append(owner.ParameterName);
            builder.Append(" where id = ANY(:");
            builder.Append(idList.ParameterName);
            builder.Append(")");
        }
    }
}
