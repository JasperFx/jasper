using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Baseline;
using Jasper.Persistence.Database;
using Jasper.Persistence.Durability;
using Jasper.Persistence.SqlServer.Schema;
using Jasper.Persistence.SqlServer.Util;
using Jasper.Transports;
using Microsoft.Data.SqlClient;
using Weasel.Core;
using Weasel.SqlServer;
using CommandExtensions = Weasel.Core.CommandExtensions;

namespace Jasper.Persistence.SqlServer.Persistence
{
    public class SqlServerEnvelopePersistence : DatabaseBackedEnvelopePersistence
    {
        private readonly SqlServerSettings _databaseSettings;
        private readonly string _findAtLargeEnvelopesSql;
        private readonly string _moveToDeadLetterStorageSql;


        public SqlServerEnvelopePersistence(SqlServerSettings databaseSettings, AdvancedSettings settings)
            : base(databaseSettings, settings, new SqlServerEnvelopeStorageAdmin(databaseSettings))
        {
            _databaseSettings = databaseSettings;
            _findAtLargeEnvelopesSql =
                $"select top {settings.RecoveryBatchSize} {DatabaseConstants.IncomingFields} from {databaseSettings.SchemaName}.{DatabaseConstants.IncomingTable} where owner_id = {TransportConstants.AnyNode} and status = '{EnvelopeStatus.Incoming}'";

            _moveToDeadLetterStorageSql = $"EXEC {_databaseSettings.SchemaName}.uspDeleteIncomingEnvelopes @IDLIST;";
        }

        public override Task DeleteIncomingEnvelopes(Envelope[] envelopes)
        {
            return _databaseSettings.CallFunction("uspDeleteIncomingEnvelopes")
                .WithIdList(_databaseSettings, envelopes).ExecuteOnce(_cancellation);
        }

        public override Task MoveToDeadLetterStorage(ErrorReport[] errors)
        {
            var table = new DataTable();
            table.Columns.Add(new DataColumn("ID", typeof(Guid)));
            foreach (var error in errors) table.Rows.Add(error.Id);

            var builder = DatabaseSettings.ToCommandBuilder();

            var list = builder.AddNamedParameter("IDLIST", table).As<SqlParameter>();
            list.SqlDbType = SqlDbType.Structured;
            list.TypeName = $"{_databaseSettings.SchemaName}.EnvelopeIdList";

            builder.Append(_moveToDeadLetterStorageSql);

            ConfigureDeadLetterCommands(errors, builder, DatabaseSettings);

            return builder.Compile().ExecuteOnce(_cancellation);
        }

        public override void Describe(TextWriter writer)
        {
            writer.WriteLine($"Sql Server Envelope Storage in Schema '{_databaseSettings.SchemaName}'");
        }

        protected override string determineOutgoingEnvelopeSql(DatabaseSettings databaseSettings,
            AdvancedSettings settings)
        {
            return
                $"select top {settings.RecoveryBatchSize} {DatabaseConstants.OutgoingFields} from {databaseSettings.SchemaName}.{DatabaseConstants.OutgoingTable} where owner_id = {TransportConstants.AnyNode} and destination = @destination";
        }

        public override Task ReassignOutgoing(int ownerId, Envelope[] outgoing)
        {
            var cmd = Session.CallFunction("uspMarkOutgoingOwnership")
                .WithIdList(DatabaseSettings, outgoing)
                .With("owner", ownerId);

            return cmd.ExecuteNonQueryAsync(_cancellation);
        }

        public override Task DiscardAndReassignOutgoing(Envelope[] discards, Envelope[] reassigned, int nodeId)
        {
            var cmd = DatabaseSettings.CallFunction("uspDiscardAndReassignOutgoing")
                .WithIdList(DatabaseSettings, discards, "discards")
                .WithIdList(DatabaseSettings, reassigned, "reassigned")
                .With("ownerId", nodeId);

            return cmd.ExecuteOnce(_cancellation);
        }

        public override Task DeleteOutgoing(Envelope[] envelopes)
        {
            return DatabaseSettings.CallFunction("uspDeleteOutgoingEnvelopes")
                .WithIdList(DatabaseSettings, envelopes).ExecuteOnce(_cancellation);
        }

        public override Task<IReadOnlyList<Envelope>> LoadPageOfLocallyOwnedIncoming()
        {
            return Session.CreateCommand(_findAtLargeEnvelopesSql)
                .FetchList(r => ReadIncoming(r));
        }

        public override Task ReassignIncoming(int ownerId, IReadOnlyList<Envelope> incoming)
        {
            return Session.CallFunction("uspMarkIncomingOwnership")
                .WithIdList(_databaseSettings, incoming)
                .With("owner", ownerId)
                .ExecuteNonQueryAsync(_cancellation);
        }
    }
}
