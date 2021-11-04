using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Jasper.Logging;
using Jasper.Persistence.Database;
using Jasper.Persistence.Durability;
using Jasper.Persistence.SqlServer.Persistence;
using Jasper.Persistence.SqlServer.Util;
using Microsoft.Data.SqlClient;
using Weasel.Core;
using Weasel.SqlServer;
using Weasel.SqlServer.Procedures;
using Weasel.SqlServer.Tables;
using CommandExtensions = Weasel.Core.CommandExtensions;

namespace Jasper.Persistence.SqlServer.Schema
{
    internal class DeadLettersTable : Table
    {
        public DeadLettersTable(string schemaName) : base(new DbObjectName(schemaName, DatabaseConstants.DeadLetterTable))
        {
            AddColumn<Guid>(DatabaseConstants.Id).AsPrimaryKey();

            AddColumn<DateTimeOffset>(DatabaseConstants.ExecutionTime).DefaultValueByExpression("NULL");
            AddColumn<int>(DatabaseConstants.Attempts).DefaultValue(0);
            AddColumn(DatabaseConstants.Body, "varbinary(max)").NotNull();

            AddColumn<Guid>(DatabaseConstants.CausationId);
            AddColumn<Guid>(DatabaseConstants.CorrelationId);
            AddColumn<string>(DatabaseConstants.SagaId);
            AddColumn<string>(DatabaseConstants.MessageType).NotNull();
            AddColumn<string>(DatabaseConstants.ContentType);
            AddColumn<string>(DatabaseConstants.ReplyRequested);
            AddColumn<bool>(DatabaseConstants.AckRequested);
            AddColumn<string>(DatabaseConstants.ReplyUri);
            AddColumn<string>(DatabaseConstants.ReceivedAt);

            AddColumn(DatabaseConstants.Source, "varchar(250)");
            AddColumn(DatabaseConstants.Explanation, "varchar(max)");
            AddColumn(DatabaseConstants.ExceptionText, "varchar(max)");
            AddColumn(DatabaseConstants.ExceptionType, "varchar(max)");
            AddColumn(DatabaseConstants.ExceptionMessage, "varchar(max)");
        }
    }

    internal class EnvelopeIdTable : TableType {
        public EnvelopeIdTable(string schemaName) : base(new DbObjectName(schemaName, "EnvelopeIdList"))
        {
            AddColumn(DatabaseConstants.Id, "UNIQUEIDENTIFIER");
        }
    }

    internal class JasperStoredProcedure : StoredProcedure
    {
        internal static string ReadText(DatabaseSettings databaseSettings, string fileName)
        {
            return Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(typeof(SqlServerEnvelopeStorageAdmin), fileName)
                .ReadAllText().Replace("%SCHEMA%", databaseSettings.SchemaName);
        }

        public JasperStoredProcedure(string fileName, DatabaseSettings settings) : base(new DbObjectName(settings.SchemaName, Path.GetFileNameWithoutExtension(fileName)), ReadText(settings, fileName))
        {
        }
    }

    public class SqlServerEnvelopeStorageAdmin : IEnvelopeStorageAdmin
    {
        private readonly Database.DatabaseSettings _settings;
        private readonly ISchemaObject[] _schemaObjects;

        public SqlServerEnvelopeStorageAdmin(Database.DatabaseSettings settings)
        {
            _settings = settings;

            _schemaObjects = new ISchemaObject[]
            {
                new OutgoingEnvelopeTable(settings.SchemaName),
                new IncomingEnvelopeTable(settings.SchemaName),
                new DeadLettersTable(settings.SchemaName),
                new EnvelopeIdTable(settings.SchemaName),
                new JasperStoredProcedure("uspDeleteIncomingEnvelopes.sql", settings),
                new JasperStoredProcedure("uspDeleteOutgoingEnvelopes.sql", settings),
                new JasperStoredProcedure("uspDiscardAndReassignOutgoing.sql", settings),
                new JasperStoredProcedure("uspMarkIncomingOwnership.sql", settings),
                new JasperStoredProcedure("uspMarkOutgoingOwnership.sql", settings),
            };
        }

        public async Task DropAll()
        {
            await using var conn = new SqlConnection(_settings.ConnectionString);
            await conn.OpenAsync();

            foreach (var schemaObject in _schemaObjects)
            {
                await schemaObject.Drop(conn);
            }
        }

        public async Task CreateAll()
        {
            await using var conn = new SqlConnection(_settings.ConnectionString);
            await conn.OpenAsync();

            var patch = await SchemaMigration.Determine(conn, _schemaObjects);

            await patch.ApplyAll(conn, new DdlRules(), AutoCreate.CreateOrUpdate);
        }



        public async Task RecreateAll()
        {
            await using var conn = new SqlConnection(_settings.ConnectionString);
            await conn.OpenAsync();

            SchemaMigration patch = null;
            try
            {
                patch = await SchemaMigration.Determine(conn, _schemaObjects);
            }
            catch (Exception)
            {
                await Task.Delay(250);
                patch = await SchemaMigration.Determine(conn, _schemaObjects);
            }

            if (patch.Difference != SchemaPatchDifference.None)
            {
                await patch.ApplyAll(conn, new DdlRules(), AutoCreate.All);
            }
        }

        async Task IEnvelopeStorageAdmin.ClearAllPersistedEnvelopes()
        {
            await using var conn = new SqlConnection(_settings.ConnectionString);
            await conn.OpenAsync();
            var tx = (SqlTransaction) await conn.BeginTransactionAsync();
            await conn.CreateCommand($"delete from {_settings.SchemaName}.{DatabaseConstants.OutgoingTable}", tx).ExecuteNonQueryAsync();
            await conn.CreateCommand($"delete from {_settings.SchemaName}.{DatabaseConstants.IncomingTable}", tx).ExecuteNonQueryAsync();
            await conn.CreateCommand($"delete from {_settings.SchemaName}.{DatabaseConstants.DeadLetterTable}", tx).ExecuteNonQueryAsync();

            await tx.CommitAsync();
        }

        async Task IEnvelopeStorageAdmin.RebuildSchemaObjects()
        {
            await using var conn = new SqlConnection(_settings.ConnectionString);
            await conn.OpenAsync();

            var patch = await SchemaMigration.Determine(conn, _schemaObjects);

            if (patch.Difference != SchemaPatchDifference.None)
            {
                await patch.ApplyAll(conn, new DdlRules(), AutoCreate.CreateOrUpdate);
            }

            await truncateEnvelopeData(conn);
        }

        private async Task truncateEnvelopeData(SqlConnection conn)
        {
            try
            {
                var tx = (SqlTransaction) await conn.BeginTransactionAsync();
                await conn.CreateCommand($"delete from {_settings.SchemaName}.{DatabaseConstants.OutgoingTable}", tx).ExecuteNonQueryAsync();
                await conn.CreateCommand($"delete from {_settings.SchemaName}.{DatabaseConstants.IncomingTable}", tx).ExecuteNonQueryAsync();
                await conn.CreateCommand($"delete from {_settings.SchemaName}.{DatabaseConstants.DeadLetterTable}", tx).ExecuteNonQueryAsync();

                await tx.CommitAsync();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    "Failure trying to execute the truncate table statements for the envelope storage", e);
            }
        }

        string IEnvelopeStorageAdmin.CreateSql()
        {
            var writer = new StringWriter();

            writer.WriteLine(
                $@"
IF EXISTS (SELECT name FROM sys.schemas WHERE name = N'{_settings.SchemaName}')
   BEGIN
      PRINT 'Dropping the {_settings.SchemaName} schema'
      DROP SCHEMA [{_settings.SchemaName}]
END
GO
PRINT '    Creating the {_settings.SchemaName} schema'
GO
CREATE SCHEMA [{_settings.SchemaName}] AUTHORIZATION [dbo]
GO

");

            var rules = new DdlRules();
            foreach (var table in _schemaObjects)
            {
                table.WriteCreateStatement(rules, writer);
            }


            return writer.ToString();
        }

        public async Task<PersistedCounts> GetPersistedCounts()
        {
            var counts = new PersistedCounts();

            await using var conn = _settings.CreateConnection();
            await conn.OpenAsync();


            await using var reader = await conn
                .CreateCommand(
                    $"select status, count(*) from {_settings.SchemaName}.{DatabaseConstants.IncomingTable} group by status")
                .ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var status = Enum.Parse<EnvelopeStatus>(await reader.GetFieldValueAsync<string>(0));
                var count = await reader.GetFieldValueAsync<int>(1);

                switch (status)
                {
                    case EnvelopeStatus.Incoming:
                        counts.Incoming = count;
                        break;
                    case EnvelopeStatus.Scheduled:
                        counts.Scheduled = count;
                        break;
                }
            }

            counts.Outgoing = (int) await CommandExtensions.CreateCommand(conn, $"select count(*) from {_settings.SchemaName}.{DatabaseConstants.OutgoingTable}").ExecuteScalarAsync();


            return counts;
        }

        public async Task<IReadOnlyList<Envelope>> AllIncomingEnvelopes()
        {
            await using var conn = _settings.CreateConnection();
            await conn.OpenAsync();

            return await conn
                .CreateCommand(
                    $"select {DatabaseConstants.IncomingFields} from {_settings.SchemaName}.{DatabaseConstants.IncomingTable}")
                .FetchList(r => DatabaseBackedEnvelopePersistence.ReadIncoming(r));
        }

        public async Task<IReadOnlyList<Envelope>> AllOutgoingEnvelopes()
        {
            await using var conn = _settings.CreateConnection();
            await conn.OpenAsync();

            return await conn
                .CreateCommand(
                    $"select {DatabaseConstants.OutgoingFields} from {_settings.SchemaName}.{DatabaseConstants.OutgoingTable}")
                .FetchList(r => DatabaseBackedEnvelopePersistence.ReadOutgoing(r));
        }

        public Task ReleaseAllOwnership()
        {
            using var conn = _settings.CreateConnection();
            conn.Open();

            conn.CreateCommand($"update {_settings.SchemaName}.{DatabaseConstants.IncomingTable} set owner_id = 0;update {_settings.SchemaName}.{DatabaseConstants.OutgoingTable} set owner_id = 0");

            return Task.CompletedTask;
        }
    }
}
