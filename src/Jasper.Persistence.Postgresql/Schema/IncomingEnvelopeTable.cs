using System;
using Jasper.Persistence.Database;
using Weasel.Core;
using Weasel.Postgresql.Tables;

namespace Jasper.Persistence.Postgresql.Schema
{
    internal class IncomingEnvelopeTable : Table
    {

        public IncomingEnvelopeTable(string schemaName) : base(new DbObjectName(schemaName, DatabaseConstants.IncomingTable))
        {
            AddColumn<Guid>(DatabaseConstants.Id).AsPrimaryKey();
            AddColumn<string>(DatabaseConstants.Status).NotNull();
            AddColumn<int>(DatabaseConstants.OwnerId).NotNull();
            AddColumn<DateTimeOffset>(DatabaseConstants.ExecutionTime).DefaultValueByExpression("NULL");
            AddColumn<int>(DatabaseConstants.Attempts);
            AddColumn(DatabaseConstants.Body, "bytea").NotNull();
        }
    }
}