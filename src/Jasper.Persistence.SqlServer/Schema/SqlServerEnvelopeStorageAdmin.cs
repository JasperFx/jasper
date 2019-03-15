using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Persistence.SqlServer.Persistence;
using Jasper.Persistence.SqlServer.Util;

namespace Jasper.Persistence.SqlServer.Schema
{
    public class SqlServerEnvelopeStorageAdmin : SqlServerAccess,IEnvelopeStorageAdmin
    {
        private readonly string _connectionString;

        private readonly string[] _creationOrder =
        {
            "Creation.sql",
            "uspDeleteIncomingEnvelopes.sql",
            "uspDeleteOutgoingEnvelopes.sql",
            "uspDiscardAndReassignOutgoing.sql",
            "uspMarkIncomingOwnership.sql",
            "uspMarkOutgoingOwnership.sql"
        };

        public SqlServerEnvelopeStorageAdmin(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlServerEnvelopeStorageAdmin(string connectionString, string schemaName)
        {
            _connectionString = connectionString;
            SchemaName = schemaName;
        }

        public string SchemaName { get; set; } = "dbo";

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
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                execute(conn, "Drop.sql");
            }
        }

        private void execute(SqlConnection conn, string filename)
        {
            var sql = toScript(filename, SchemaName);

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
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                buildSchemaIfNotExists(conn);

                foreach (var file in _creationOrder) execute(conn, file);
            }
        }

        public void RecreateAll()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                execute(conn, "Drop.sql");

                buildSchemaIfNotExists(conn);

                foreach (var file in _creationOrder) execute(conn, file);
            }
        }

        private void buildSchemaIfNotExists(SqlConnection conn)
        {
            var count = conn.CreateCommand("select count(*) from sys.schemas where name = @name")
                .With("name", SchemaName).ExecuteScalar().As<int>();

            if (count == 0) conn.CreateCommand($"CREATE SCHEMA [{SchemaName}] AUTHORIZATION [dbo]").ExecuteNonQuery();
        }

        void IEnvelopeStorageAdmin.ClearAllPersistedEnvelopes()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                try
                {
                    conn.CreateCommand($"truncate table {SchemaName}.jasper_outgoing_envelopes;truncate table {SchemaName}.jasper_incoming_envelopes;truncate table {SchemaName}.jasper_dead_letters;").ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Failure trying to execute the truncate table statements for the envelope storage", e);
                }

            }
        }

        void IEnvelopeStorageAdmin.RebuildSchemaObjects()
        {
            DropAll();
            RecreateAll();
        }

        string IEnvelopeStorageAdmin.CreateSql()
        {
            var writer = new StringWriter();
            writer.WriteLine(
                $@"
IF EXISTS (SELECT name FROM sys.schemas WHERE name = N'{SchemaName}')
   BEGIN
      PRINT 'Dropping the {SchemaName} schema'
      DROP SCHEMA [{SchemaName}]
END
GO
PRINT '    Creating the {SchemaName} schema'
GO
CREATE SCHEMA [{SchemaName}] AUTHORIZATION [dbo]
GO

");

            foreach (var file in _creationOrder)
            {
                var sql = toScript(file, SchemaName);
                writer.WriteLine(sql);
                writer.WriteLine("GO");
                writer.WriteLine();
            }



            return writer.ToString();

        }

        public async Task<PersistedCounts> GetPersistedCounts()
        {
            var counts = new PersistedCounts();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();


                using (var reader = await conn
                    .CreateCommand(
                        $"select status, count(*) from {SchemaName}.{IncomingTable} group by status")
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
                    .CreateCommand($"select count(*) from {SchemaName}.{OutgoingTable}").ExecuteScalarAsync();
            }


            return counts;
        }

        public async Task<ErrorReport> LoadDeadLetterEnvelope(Guid id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var cmd = conn.CreateCommand(
                    $"select body, explanation, exception_text, exception_type, exception_message, source, message_type, id from {SchemaName}.{DeadLetterTable} where id = @id");
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
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                return await conn.CreateCommand($"select body, status, owner_id from {SchemaName}.{IncomingTable}").ExecuteToEnvelopes();
            }
        }

        public async Task<Envelope[]> AllOutgoingEnvelopes()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                return await conn.CreateCommand($"select body, status, owner_id from {SchemaName}.{OutgoingTable}").ExecuteToEnvelopes();
            }
        }
    }
}
