using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Persistence.Database;
using Jasper.Persistence.SqlServer.Persistence;
using Jasper.Persistence.SqlServer.Util;

namespace Jasper.Persistence.SqlServer.Schema
{
    public class SqlServerEnvelopeStorageAdmin : DataAccessor,IEnvelopeStorageAdmin
    {
        private readonly Database.DatabaseSettings _settings;

        private readonly string[] _creationOrder =
        {
            "Creation.sql",
            "uspDeleteIncomingEnvelopes.sql",
            "uspDeleteOutgoingEnvelopes.sql",
            "uspDiscardAndReassignOutgoing.sql",
            "uspMarkIncomingOwnership.sql",
            "uspMarkOutgoingOwnership.sql"
        };

        public SqlServerEnvelopeStorageAdmin(Database.DatabaseSettings settings)
        {
            _settings = settings;
        }

        public static string ToCreationScript(string schema)
        {
            // TODO -- more here
            return toScript("Creation.sql", schema);
        }

        public static string ToDropScript(string schema)
        {
            return toScript("Drop.sql", schema);
        }

        private static string toScript(string fileName, string schema)
        {
            var text = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(SqlServerEnvelopeStorageAdmin), fileName)
                .ReadAllText();

            return text.Replace("%SCHEMA%", schema);
        }

        public void DropAll()
        {
            execute("Drop.sql");
        }

        private void execute(params string[] filenames)
        {
            using (var conn = _settings.CreateConnection())
            {
                conn.Open();

                foreach (var filename in filenames)
                {
                    execute(filename, conn);
                }


            }
        }

        private void execute(string filename, DbConnection conn)
        {
            var sql = toScript(filename, _settings.SchemaName);

            try
            {
                conn.RunSql(sql);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Failure trying to execute:\n\n" + sql, e);
            }
        }

        private void execute(SqlConnection conn, string filename)
        {
            var sql = toScript(filename, _settings.SchemaName);

            try
            {
                conn.CreateCommand(sql).ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Failure trying to execute:\n\n" + sql, e);
            }
        }

        public void CreateAll()
        {
            using (var conn = _settings.CreateConnection())
            {
                conn.Open();

                buildSchemaIfNotExists(conn);

                foreach (var file in _creationOrder)
                {
                    execute(file, conn);
                }
            }
        }

        public void RecreateAll()
        {
            using (var conn = _settings.CreateConnection())
            {
                conn.Open();

                execute("Drop.sql", conn);

                buildSchemaIfNotExists(conn);

                foreach (var file in _creationOrder)
                {
                    execute(file, conn);
                }
            }
        }

        private void buildSchemaIfNotExists(DbConnection conn)
        {
            var count = conn.CreateCommand("select count(*) from sys.schemas where name = @name")
                .With("name", _settings.SchemaName).ExecuteScalar().As<int>();

            if (count == 0) conn.CreateCommand($"CREATE SCHEMA [{_settings.SchemaName}] AUTHORIZATION [dbo]").ExecuteNonQuery();
        }

        void IEnvelopeStorageAdmin.ClearAllPersistedEnvelopes()
        {
            var sql = $"truncate table {_settings.SchemaName}.jasper_outgoing_envelopes;truncate table {_settings.SchemaName}.jasper_incoming_envelopes;truncate table {_settings.SchemaName}.jasper_dead_letters;";
            _settings.ExecuteSql(sql);
        }

        void IEnvelopeStorageAdmin.RebuildSchemaObjects()
        {
            DropAll();
            RecreateAll();
        }

        string IEnvelopeStorageAdmin.CreateSql()
        {
            var writer = new StringWriter();

            // TODO -- move this to an embedded file ot make it easier
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

            foreach (var file in _creationOrder)
            {
                var sql = toScript(file, _settings.SchemaName);
                writer.WriteLine(sql);
                writer.WriteLine("GO");
                writer.WriteLine();
            }



            return writer.ToString();

        }

        public async Task<PersistedCounts> GetPersistedCounts()
        {
            var counts = new PersistedCounts();

            using (var conn = _settings.CreateConnection())
            {
                await conn.OpenAsync();


                using (var reader = await conn
                    .CreateCommand(
                        $"select status, count(*) from {_settings.SchemaName}.{IncomingTable} group by status")
                    .ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var status = await reader.GetFieldValueAsync<string>(0);
                        var count = await reader.GetFieldValueAsync<int>(1);

                        if (status == TransportConstants.Incoming)
                            counts.Incoming = count;
                        else if (status == TransportConstants.Scheduled) counts.Scheduled = count;
                    }
                }

                counts.Outgoing = (int) await conn
                    .CreateCommand($"select count(*) from {_settings.SchemaName}.{OutgoingTable}").ExecuteScalarAsync();
            }


            return counts;
        }

        public async Task<ErrorReport> LoadDeadLetterEnvelope(Guid id)
        {
            using (var conn = _settings.CreateConnection())
            {
                await conn.OpenAsync();

                var cmd = conn.CreateCommand(
                    $"select body, explanation, exception_text, exception_type, exception_message, source, message_type, id from {_settings.SchemaName}.{DeadLetterTable} where id = @id");
                cmd.AddNamedParameter("id", id);

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

        public async Task<Envelope[]> AllIncomingEnvelopes()
        {
            using (var conn = _settings.CreateConnection())
            {
                await conn.OpenAsync();

                return await conn.CreateCommand($"select body, status, owner_id from {_settings.SchemaName}.{IncomingTable}").ExecuteToEnvelopes();
            }
        }

        public async Task<Envelope[]> AllOutgoingEnvelopes()
        {
            using (var conn = _settings.CreateConnection())
            {
                await conn.OpenAsync();

                return await conn.CreateCommand($"select body, status, owner_id from {_settings.SchemaName}.{OutgoingTable}").ExecuteToEnvelopes();
            }
        }
    }
}
