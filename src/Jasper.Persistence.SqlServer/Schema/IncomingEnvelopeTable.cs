﻿using System;
using Jasper.Persistence.Database;
using Weasel.Core;
using Weasel.SqlServer.Tables;

namespace Jasper.Persistence.SqlServer.Schema;

internal class IncomingEnvelopeTable : Table
{
    public IncomingEnvelopeTable(string schemaName) : base(
        new DbObjectName(schemaName, DatabaseConstants.IncomingTable))
    {
        AddColumn<Guid>(DatabaseConstants.Id).AsPrimaryKey();
        AddColumn("status", "varchar(25)").NotNull();
        AddColumn<int>(DatabaseConstants.OwnerId).NotNull();
        AddColumn<DateTimeOffset>(DatabaseConstants.ExecutionTime).DefaultValueByExpression("NULL");
        AddColumn<int>(DatabaseConstants.Attempts).DefaultValue(0);
        AddColumn(DatabaseConstants.Body, "varbinary(max)").NotNull();

        AddColumn<string>(DatabaseConstants.CausationId);
        AddColumn<string>(DatabaseConstants.CorrelationId);
        AddColumn<string>(DatabaseConstants.SagaId);
        AddColumn<string>(DatabaseConstants.ParentId);
        AddColumn(DatabaseConstants.MessageType, "varchar(250)").NotNull();
        AddColumn<string>(DatabaseConstants.ContentType);
        AddColumn(DatabaseConstants.ReplyRequested, "varchar(250)");
        AddColumn<bool>(DatabaseConstants.AckRequested);
        AddColumn<string>(DatabaseConstants.ReplyUri);
        AddColumn<string>(DatabaseConstants.ReceivedAt);
    }
}
