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
    internal class OutgoingEnvelopeTable : Table
    {

        public OutgoingEnvelopeTable(string schemaName) : base(new DbObjectName(schemaName, DatabaseConstants.OutgoingTable))
        {
            AddColumn<Guid>(DatabaseConstants.Id).AsPrimaryKey();
            AddColumn<int>(DatabaseConstants.OwnerId).NotNull();
            AddColumn(DatabaseConstants.Destination, "varchar(250)").NotNull();
            AddColumn<DateTimeOffset>(DatabaseConstants.DeliverBy);
            AddColumn(DatabaseConstants.Body, "varbinary(max)").NotNull();
        }
    }

    internal class IncomingEnvelopeTable : Table
    {

        public IncomingEnvelopeTable(string schemaName) : base(new DbObjectName(schemaName, DatabaseConstants.IncomingTable))
        {
            AddColumn<Guid>(DatabaseConstants.Id).AsPrimaryKey();
            AddColumn("status", "varchar(25)").NotNull();
            AddColumn<int>(DatabaseConstants.OwnerId).NotNull();
            AddColumn<DateTimeOffset>(DatabaseConstants.ExecutionTime).DefaultValueByExpression("NULL");
            AddColumn<int>(DatabaseConstants.Attempts);
            AddColumn(DatabaseConstants.Body, "varbinary(max)").NotNull();
        }
    }

    internal class DeadLettersTable : Table
    {
        public DeadLettersTable(string schemaName) : base(new DbObjectName(schemaName, DatabaseConstants.DeadLetterTable))
        {
            AddColumn<Guid>(DatabaseConstants.Id).AsPrimaryKey();
            AddColumn(DatabaseConstants.Source, "varchar(250)");
            AddColumn(DatabaseConstants.MessageType, "varchar(max)");
            AddColumn(DatabaseConstants.Explanation, "varchar(max)");
            AddColumn(DatabaseConstants.ExceptionText, "varchar(max)");
            AddColumn(DatabaseConstants.ExceptionType, "varchar(max)");
            AddColumn(DatabaseConstants.ExceptionMessage, "varchar(max)");
            AddColumn(DatabaseConstants.Body, "varbinary(max)").NotNull();
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

        public async Task<ErrorReport> LoadDeadLetterEnvelope(Guid id)
        {
            using (var conn = _settings.CreateConnection())
            {
                await conn.OpenAsync();

                var cmd = conn.CreateCommand(
                    $"select body, explanation, exception_text, exception_type, exception_message, source, message_type, id from {_settings.SchemaName}.{DatabaseConstants.DeadLetterTable} where id = @id");
                cmd.AddNamedParameter(DatabaseConstants.Id, id);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync()) return null;


                    var report = new ErrorReport
                    {
                        RawData = await reader.GetFieldValueAsync<byte[]>(0),
                        Explanation = await reader.GetFieldValueAsync<string>(1),
                        ExceptionText = await reader.GetFieldValueAsync<string>(2),
                        ExceptionType = await reader.GetFieldValueAsync<string>(3),
                        ExceptionMessage = await reader.GetFieldValueAsync<string>(4),
                        Source = await reader.GetFieldValueAsync<string>(5),
                        MessageType = await reader.GetFieldValueAsync<string>(6),
                        Id = await reader.GetFieldValueAsync<Guid>(7)
                    };

                    return report;
                }
            }
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
