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
    public class DeleteEnvelopes : IStorageOperation
    {
        private readonly Guid[] _idlist;
        private readonly DbObjectName _table;

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

        public void ConfigureCommand(CommandBuilder builder, IMartenSession session)
        {
            var idList = builder.AddParameter(_idlist, NpgsqlDbType.Array | NpgsqlDbType.Uuid);
            builder.Append("delete from ");
            builder.Append(_table.QualifiedName);
            builder.Append(" where id = ANY(:");
            builder.Append(idList.ParameterName);
            builder.Append(")");
        }

        public void Postprocess(DbDataReader reader, IList<Exception> exceptions)
        {

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
