using System;
using Jasper.Persistence.Database;
using Weasel.Core;
using Weasel.Postgresql.Tables;

namespace Jasper.Persistence.Postgresql.Schema
{
    internal class DeadLettersTable : Table
    {
        public DeadLettersTable(string schemaName) : base(new DbObjectName(schemaName, DatabaseConstants.DeadLetterTable))
        {
            AddColumn<Guid>(DatabaseConstants.Id).AsPrimaryKey();
            AddColumn<string>(DatabaseConstants.Source);
            AddColumn<string>(DatabaseConstants.MessageType);
            AddColumn<string>(DatabaseConstants.Explanation);
            AddColumn<string>(DatabaseConstants.ExceptionText);
            AddColumn<string>(DatabaseConstants.ExceptionType);
            AddColumn<string>(DatabaseConstants.ExceptionMessage);
            AddColumn(DatabaseConstants.Body, "bytea").NotNull();
        }
    }
}