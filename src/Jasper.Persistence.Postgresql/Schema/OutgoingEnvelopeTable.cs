using System;
using Jasper.Persistence.Database;
using Weasel.Core;
using Weasel.Postgresql.Tables;

namespace Jasper.Persistence.Postgresql.Schema
{
    internal class OutgoingEnvelopeTable : Table
    {

        public OutgoingEnvelopeTable(string schemaName) : base(new DbObjectName(schemaName, DatabaseConstants.OutgoingTable))
        {
            AddColumn<Guid>(DatabaseConstants.Id).AsPrimaryKey();
            AddColumn<int>(DatabaseConstants.OwnerId).NotNull();
            AddColumn<string>(DatabaseConstants.Destination).NotNull();
            AddColumn<DateTimeOffset>(DatabaseConstants.DeliverBy);
            AddColumn(DatabaseConstants.Body, "bytea").NotNull();
        }
    }
}